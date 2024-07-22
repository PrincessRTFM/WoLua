using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Api;
using PrincessRTFM.WoLua.Ui.Chat;

namespace PrincessRTFM.WoLua.Lua;

// Declared as `partial` because of the compile-time Regex generation feature
public sealed partial class ScriptContainer: IDisposable {
	public const CoreModules ScriptModules = CoreModules.None
#if DEBUG
		| CoreModules.Debug
#endif
		| CoreModules.Metatables
		| CoreModules.GlobalConsts
		| CoreModules.TableIterators
		| CoreModules.String
		| CoreModules.Table
		| CoreModules.Basic
		| CoreModules.Math
		| CoreModules.Bit32
		| CoreModules.OS_Time
		| CoreModules.LoadMethods
		| CoreModules.ErrorHandling
		| CoreModules.Json
		| CoreModules.Dynamic;
	public const string FatalErrorMessage = "The lua engine has encountered a fatal error. Please send your dalamud.log file to the developer and restart your game.";

	#region Path normalisation

	private static string aggregator(string accumulated, Regex pattern) => pattern.Replace(accumulated, string.Empty);

	[GeneratedRegex(@"\s+", RegexOptions.Compiled)]
	public static partial Regex AllWhitespace();

	public static readonly Regex PluginNamePrefix = new("^" + Regex.Escape($"{Plugin.Name}."), RegexOptions.IgnoreCase | RegexOptions.Compiled);
	public static readonly Regex PluginNameSuffix = new(Regex.Escape($".{Plugin.Name}") + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	internal static readonly Regex[] standardRemovals = [
		AllWhitespace(),
	];
	internal static readonly Regex[] experimentalRemovals = [
		PluginNamePrefix,
		PluginNameSuffix,
	];

	public static string NameToSlug(in string name, in bool forceNormalisation = false) {
		IEnumerable<Regex> regexen = standardRemovals;
		if (forceNormalisation || Service.Configuration.ExperimentalPathNormalisation)
			regexen = regexen.Concat(experimentalRemovals);
		return regexen.Aggregate(name, aggregator);
	}

	#endregion

	public string InternalName { get; }
	public string PrettyName { get; }
	public string SourcePath { get; }

	#region Direct invocation command

	public bool CommandRegistered { get; private set; } = false;
	private void redirectCommandInvocation(string command, string argline) => Service.ScriptManager.Invoke(this.InternalName, argline);
	public bool RegisterCommand() {
		if (this.Disposed)
			return false;
		if (this.CommandRegistered)
			return true;

		string shortform = $"/{Service.Configuration.DirectInvocationCommandPrefix}{this.InternalName}".ToLower();
		this.CommandRegistered = Service.CommandManager.AddHandler(shortform, new(this.redirectCommandInvocation) {
			HelpMessage = $"Run the {this.InternalName} script from {Plugin.Name}",
			ShowInHelp = false,
		});
		if (this.CommandRegistered)
			this.Log($"Registered {shortform}", LogTag.PluginCore, true);
		else
			this.Log($"Unable to register direct command \"{shortform}\" with Dalamud", LogTag.PluginCore, true);
		return this.CommandRegistered;
	}
	public void UnregisterCommand() {
		if (this.CommandRegistered)
			Service.CommandManager?.RemoveHandler($"/{Service.Configuration.DirectInvocationCommandPrefix}{this.InternalName}");
		this.CommandRegistered = false;
	}

	#endregion

	public Script Engine { get; private set; } = new(ScriptModules);

	public ActionQueue ActionQueue { get; private set; }

	[LuaGlobal("Script")]
	public ScriptApi ScriptApi { get; private set; } = null!;
	[LuaGlobal("Game")]
	public GameApi GameApi { get; private set; } = null!;

	public string SourceDir => Path.GetDirectoryName(this.SourcePath)!;
	public string SourceFile => Path.GetFileName(this.SourcePath);

	public bool LoadSuccess { get; private set; } = false;
	public bool ErrorOnCall { get; private set; } = false;

	internal DynValue Callback { get; set; } = DynValue.Void;
	/// <summary>
	/// Indicates whether this script registered succesffuly and is not disposed. It may have encountered errors after registration.
	/// </summary>
	public bool Ready => !this.Disposed && this.Engine is not null && this.Callback.Type is DataType.Function;

	/// <summary>
	/// Indicates whether this script is "active" and in effect: not disposed, with a loaded engine and a valid callback, without having thrown errors.
	/// </summary>
	public bool Active => this.Ready && this.LoadSuccess && !this.ErrorOnCall;

	public ScriptContainer(string file, string name, string slug) {
		this.InternalName = slug;
		this.PrettyName = name;
		this.SourcePath = file;

		this.Engine.Options.ScriptLoader = new ScriptLoader(this.SourceDir, slug);

		this.ActionQueue = new(this);

		Type apiBase = typeof(ApiBase);
		Type self = this.GetType();
		Type[] ctorTypes = [self];
		object?[] ctorParams = [this];
		PropertyInfo[] scriptGlobals = self
			.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(p => p.PropertyType.IsAssignableTo(apiBase) && !p.PropertyType.IsAbstract && p.GetCustomAttribute<LuaGlobalAttribute>() is not null)
			.ToArray();

		foreach (PropertyInfo p in scriptGlobals) {
			LuaGlobalAttribute g = p.GetCustomAttribute<LuaGlobalAttribute>()!;
			ConstructorInfo ci = p.PropertyType.GetConstructor(ctorTypes)!;
			ApiBase o = (ApiBase)ci!.Invoke(ctorParams);
			p.SetValue(this, o);
			this.Engine.Globals[g.Name] = o;
			o.Init();
		}
		foreach (PropertyInfo p in scriptGlobals) {
			ApiBase o = (ApiBase)p.GetValue(this)!;
			o.PostInit();
		}

		try {
			this.Engine.DoFile(this.SourceFile);
			this.LoadSuccess = true;
		}
		catch (SyntaxErrorException e) {
			Service.Plugin.Error($"Syntax error in {file}", e);
		}
		catch (InterpreterException e) when (e is ScriptRuntimeException or DynamicExpressionException) {
			Service.Plugin.Error($"Initialisation error in {file}", e);
		}
		catch (InternalErrorException e) {
			Service.Plugin.Error(FatalErrorMessage, e);
		}
		catch (InterpreterException e) {
			Service.Plugin.Error($"Unknown error in {file}", e);
		}

		if (this.LoadSuccess) {
			if (!this.Ready) { // The script loaded, but they failed to set their invocation callback, so this script won't actually DO anything
				Service.Plugin.Error($"No on-execute function registered in {file} (did you forget to call Script(callback) maybe?)");
			}
		}
		else { // If there was a load error, don't try to run any callback that may have been registered, the script is in an unknown (errored) state and it might not work
			Service.Plugin.Error($"Loading failed, clearing callback for {this.PrettyName}");
			this.Dispose(); // shut everything down because this ship ain't sailing
		}
	}
	public ScriptContainer(string file) : this(file, new DirectoryInfo(Path.GetDirectoryName(file)!).Name) { }
	public ScriptContainer(string file, string name) : this(file, name, NameToSlug(name)) { }

	internal bool ReportError() {
		if (!this.LoadSuccess) {
			Service.Plugin.Error($"\"{this.PrettyName}\" ({this.InternalName}) failed to load due to an error.");
			return true;
		}
		if (!this.Ready) {
			Service.Plugin.Error($"\"{this.PrettyName}\" ({this.InternalName}) did not register a callback function.");
			return true;
		}
		if (this.ErrorOnCall) {
			Service.Plugin.Error($"\"{this.PrettyName}\" ({this.InternalName}) encountered an error during a previous call and has been disabled.");
			return true;
		}
		return false;
	}

	public bool SetCallback(DynValue func) {
		if (this.Disposed)
			return false;

		if (func.Type is DataType.Function) {
			this.Callback = func;
		}

		return this.Ready;
	}

	public void Invoke(string parameters) {
		if (this.Disposed)
			return;

		if (this.Ready) {
			try {
				this.Callback.Function.Call(parameters);
			}
			catch (InterpreterException e) when (e is ScriptRuntimeException or SyntaxErrorException or DynamicExpressionException) {
				Service.Plugin.Error("This script ran into an error and has been disabled. You will need to reload your scripts to re-enable it.", e, this.PrettyName);
				Service.Plugin.Print(e.Message, Foreground.Debug);
				this.ErrorOnCall = true;
				this.Callback = DynValue.Void;
			}
			catch (InternalErrorException e) {
				Service.Plugin.Error(FatalErrorMessage, e);
				this.ErrorOnCall = true;
				this.Callback = DynValue.Void;
			}
		}
		else if (this.ErrorOnCall) {
			Service.Plugin.Print("This script ran into an error during a previous call and has been disabled. You will need to reload your scripts to re-enable it.", Foreground.Error, this.PrettyName);
		}
		else if (this.LoadSuccess) {
			Service.Plugin.Print("This script didn't register an on-execute function, so it can't be called.", Foreground.Error, this.PrettyName);
		}
		else {
			Service.Plugin.Print("This script ran into an error while loading and has been disabled. You can reload your scripts once it's fixed to enable it.", Foreground.Error, this.PrettyName);
		}
	}

	internal void Log(string message, string tag, bool force = false) {
		if (force || this.ScriptApi.Debug.Enabled)
			Service.Log.Information($"[SCRIPT:{this.PrettyName}|{tag}] {message}");
	}

	internal void CleanTable(Table table) {
		this.Log("Recursively collecting dead keys", LogTag.Cleanup);
		Queue<Table> queue = new();
		queue.Enqueue(table);
		while (queue.TryDequeue(out Table? target)) {
			this.Log("Clearing dead keys...", LogTag.Cleanup);
			target.CollectDeadKeys();
			TablePair[] entries = target.Pairs.ToArray();
			foreach (TablePair entry in entries) {
				if (entry.Value.Type is DataType.Table) {
					this.Log($"Found subtable {entry.Key}", LogTag.Cleanup);
					queue.Enqueue(entry.Value.Table);
				}
			}
			this.Log("Table finished", LogTag.Cleanup);
		}
	}

	#region Disposable
	public bool Disposed { get; private set; } = false;
	private void dispose(bool disposing) {
		if (this.Disposed)
			return;
		this.Disposed = true;

		if (disposing) {
			this.UnregisterCommand();
			this.ActionQueue?.Dispose();
			this.ScriptApi?.Dispose();
			this.GameApi?.Dispose();
		}

		this.Log(this.GetType().Name, LogTag.Dispose, true);

		this.Callback = DynValue.Void;
		this.Engine = null!;
		this.ActionQueue = null!;
		this.ScriptApi = null!;
		this.GameApi = null!;
	}

	~ScriptContainer() {
		this.dispose(false);
	}

	public void Dispose() {
		this.dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
