namespace PrincessRTFM.WoLua.Lua;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dalamud.Logging;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Lua.Api;
using PrincessRTFM.WoLua.Ui.Chat;

public class ScriptContainer: IDisposable {
	public const CoreModules ScriptModules = CoreModules.None
#if DEBUG
		| CoreModules.Debug
		| CoreModules.Metatables
#endif
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

	public readonly string InternalName;
	public readonly string PrettyName;
	public readonly string SourcePath;

	public Script Engine { get; private set; } = new(ScriptModules);

	public ActionQueue ActionQueue { get; private set; }

	public ScriptApi ScriptApi { get; private set; }
	public GameApi GameApi { get; private set; }

	public string SourceDir => Path.GetDirectoryName(this.SourcePath)!;
	public string SourceFile => Path.GetFileName(this.SourcePath);

	public bool LoadSuccess { get; private set; } = false;
	public bool ErrorOnCall { get; private set; } = false;

	internal DynValue callback = DynValue.Void;
	public bool Ready => this.Engine is not null && this.callback.Type is DataType.Function;

	public ScriptContainer(string file, string name, string slug) {
		this.InternalName = slug;
		this.PrettyName = name;
		this.SourcePath = file;

		this.Engine.Options.ScriptLoader = new ScriptLoader(this.SourceDir);

		this.ActionQueue = new(this);

		this.ScriptApi = new(this);
		this.ScriptApi.ReloadStorage();

		this.GameApi = new(this);

		this.Engine.Options.DebugPrint = this.ScriptApi.Debug.PrintString;
		this.Engine.Options.DebugInput = this.ScriptApi.Debug.Input;
		this.Engine.Globals["Script"] = this.ScriptApi;
		this.Engine.Globals["Game"] = this.GameApi;

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
	public ScriptContainer(string file, string name) : this(file, name, name.Replace(" ", "")) { }

	public bool SetCallback(DynValue func) {
		if (this.Disposed)
			return false;

		if (func.Type is DataType.Function) {
			this.callback = func;
		}

		return this.Ready;
	}

	public void Invoke(string parameters) {
		if (this.Disposed)
			return;

		if (this.Ready) {
			try {
				this.callback.Function.Call(parameters);
			}
			catch (InterpreterException e) when (e is ScriptRuntimeException or SyntaxErrorException or DynamicExpressionException) {
				Service.Plugin.Error("This script ran into an error and has been disabled. You will need to reload your scripts to re-enable it.", e);
				Service.Plugin.Print(e.Message, Foreground.Debug);
				this.ErrorOnCall = true;
				this.callback = DynValue.Void;
			}
			catch (InternalErrorException e) {
				Service.Plugin.Error(FatalErrorMessage, e);
				this.ErrorOnCall = true;
				this.callback = DynValue.Void;
			}
		}
		else if (this.ErrorOnCall) {
			Service.Plugin.Print("This script ran into an error during a previous call and has been disabled. You will need to reload your scripts to re-enable it.", Foreground.Error);
		}
		else if (this.LoadSuccess) {
			Service.Plugin.Print("This script didn't register an on-execute function, so it can't be called.", Foreground.Error);
		}
		else {
			Service.Plugin.Print("This script ran into an error while loading and has been disabled. You can reload your scripts once it's fixed to enable it.", Foreground.Error);
		}
	}

	internal void log(string message, string tag, bool force = false) {
		if (force || this.ScriptApi.Debug.Enabled)
			PluginLog.Information($"[SCRIPT:{this.PrettyName}|{tag}] {message}");
	}

	internal void cleanTable(Table table) {
		this.log("Recursively collecting dead keys", "CLEANTABLE");
		Queue<Table> queue = new();
		queue.Enqueue(table);
		while (queue.TryDequeue(out Table? target)) {
			this.log("Clearing dead keys...", "CLEANTABLE");
			target.CollectDeadKeys();
			TablePair[] entries = target.Pairs.ToArray();
			foreach (TablePair entry in entries) {
				if (entry.Value.Type is DataType.Table) {
					this.log($"Found subtable {entry.Key}", "CLEANTABLE");
					queue.Enqueue(entry.Value.Table);
				}
			}
			this.log("Table finished", "CLEANTABLE");
		}
	}

	#region Disposable
	public bool Disposed { get; private set; } = false;
	protected virtual void Dispose(bool disposing) {
		if (this.Disposed)
			return;
		this.Disposed = true;

		if (disposing) {
			this.ActionQueue.Dispose();
			this.ScriptApi.Dispose();
			this.GameApi.Dispose();
		}

		this.log(this.GetType().Name, "DISPOSE", true);

		this.callback = DynValue.Void;
		this.Engine = null!;
		this.ActionQueue = null!;
		this.ScriptApi = null!;
		this.GameApi = null!;
	}

	~ScriptContainer() {
		this.Dispose(false);
	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
