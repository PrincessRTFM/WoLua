namespace PrincessRTFM.WoLua.Lua;

using System;
using System.IO;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Lua.Api;
using PrincessRTFM.WoLua.Ui.Chat;

/* On the queueing and execution of actions:
 * 
 * My original thought was to use a background worker thread. Every ~50ms, it would iterate all script containers, pull one action (either "process input" or "delay") from a queue, and execute it.
 * A signal or semaphore would allow the reload command to indicate that a rescan is needed, which would allow it to clear the queues and containers, rescan, and then continue processing.
 * Delays would be implemented with a millisecond-precise threshold of "don't pull the next action until this timestamp" to skip that particular script's queue.
 * 
 * Frey suggested first using the framework update event instead of a timed thread, which I'm leery of because I don't want to block the main thread, but it's otherwise pretty solid.
 * In the event, iterate each script container there and execute (probably running consecutive chat inputs together in a single update?) and then I don't have to handle waiting between iterations myself.
 * My only concern (aside from blocking the thread) is running chat inputs TOO close together - does the game ratelimit that?
 * 
 * Frey's second suggestion was to just use `Task`s and let the runtime thread pool deal with things. The task objects would need to handle an individual container's queue and I'd have to hold them all.
 * They'd need to be cancelled when reloading or shutting down, although I don't think I'd need to join their threads at least.
 * To implement delays, I could (theoretically, at least) just call `Thread.Sleep()` within the runner.
 */

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

	public Script Engine { get; private set; } = new(ScriptModules);
	public readonly string InternalName;
	public readonly string PrettyName;
	public readonly string SourcePath;

	public ScriptApi ScriptApi { get; private set; }
	public GameApi GameApi { get; private set; }

	public string SourceDir => Path.GetDirectoryName(this.SourcePath)!;
	public string SourceFile => Path.GetFileName(this.SourcePath);

	public bool LoadSuccess { get; private set; } = false;
	public bool ErrorOnCall { get; private set; } = false;

	internal DynValue callback = DynValue.Void;
	public bool Ready => this.callback.Type is DataType.Function;

	public ScriptContainer(string file, string name, string slug) {
		this.InternalName = slug;
		this.PrettyName = name;
		this.SourcePath = file;

		this.Engine.Options.ScriptLoader = new ScriptLoader(this.SourceDir);

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
			this.callback = DynValue.Void;
		}
	}
	public ScriptContainer(string file) : this(file, new DirectoryInfo(Path.GetDirectoryName(file)!).Name) { }
	public ScriptContainer(string file, string name) : this(file, name, name.Replace(" ", "")) { }

	public bool SetCallback(DynValue func) {
		if (this.disposed)
			return false;

		if (func.Type is DataType.Function) {
			this.callback = func;
		}

		return this.Ready;
	}

	public void Invoke(string parameters) {
		if (this.disposed)
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

	#region Disposable
	private bool disposed;
	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			this.ScriptApi.Dispose();
			this.GameApi.Dispose();
		}

		this.Engine = null!;
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
