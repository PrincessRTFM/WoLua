namespace PrincessRTFM.WoLua.Lua.Api;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using ImGuiNET;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Lua.Actions;
using PrincessRTFM.WoLua.Lua.Api.Script;

// This API is for all for everything that doesn't relate to the actual game itself.
// It also contains script-specific and per-script functionality, like persistent storage.
public class ScriptApi: ApiBase {
	#region Non-API functionality

	[MoonSharpHidden]
	internal ScriptApi(ScriptContainer source) : base(source, "CORE") {
		this.Storage = new(source.Engine);
		this.StoragePath = Path.ChangeExtension(Path.Combine(Service.Interface.GetPluginConfigDirectory(), this.Owner.InternalName), "json");
		this.Debug = new(this.Owner);
		this.Keys = new(this.Owner);
	}

	protected readonly string StoragePath;

	protected override void Dispose(bool disposing) {
		if (this.Disposed)
			return;

		if (disposing) {
			this.Debug.Dispose();
			this.Keys.Dispose();
		}

		base.Dispose(disposing);

		this.Storage = null!;
		this.Debug = null!;
		this.Keys = null!;
	}

	#endregion

	#region Sub-API access

	public DebugApi Debug { get; private set; }

	public KeysApi Keys { get; private set; }

	#endregion

	#region Storage

	public Table Storage { get; protected set; }

	public bool DeleteStorage() {
		if (this.Disposed)
			return false;

		this.Storage.Clear();
		this.Log($"Deleting {this.StoragePath}", "STORAGE");
		try {
			if (File.Exists(this.StoragePath))
				File.Delete(this.StoragePath);
			return true;
		}
		catch (Exception err) {
			Service.Plugin.Error($"Failed to delete disk storage for {this.Owner.PrettyName}", err);
			return false;
		}
	}

	public bool SaveStorage() {
		if (this.Disposed)
			return false;

		this.Log($"Writing to {this.StoragePath}", "STORAGE");
		try {
			File.WriteAllText(this.StoragePath, this.Storage.TableToJson());
			return true;
		}
		catch (Exception err) {
			Service.Plugin.Error($"Failed to save storage for {this.Owner.PrettyName}", err);
			return false;
		}
	}

	public bool? ReloadStorage() {
		if (this.Disposed)
			return false;

		this.Log($"Loading from {this.StoragePath}", "STORAGE");
		try {
			string json = File.ReadAllText(this.StoragePath);
			Table loaded = JsonTableConverter.JsonToTable(json, this.Owner.Engine);
			if (loaded is null)
				return null;
			this.Storage = loaded;
			return true;
		}
		catch (FileNotFoundException) {
			this.Log("No disk storage found", "STORAGE");
			return false;
		}
		catch (Exception err) {
			Service.Plugin.Error($"Failed to load storage for {this.Owner.PrettyName}", err);
			return null;
		}
	}

	public void SetStorage(Table update) {
		if (this.Disposed)
			return;

		this.Log("Replacing script storage", "STORAGE");
		Table store = new(this.Owner.Engine);
		foreach (TablePair item in update.Pairs) {
			store[item.Key] = item.Value;
		}
		this.Storage = store;
	}

	#endregion

	#region Common strings

	public static string PluginCommand => Service.Plugin.Command;

	public string Name => this.Owner.InternalName;

	public string Title => this.Owner.PrettyName;

	public string CallSelfCommand => PluginCommand + " call " + this.Name;

	#endregion

	#region Action queueing

	public int QueueSize => this.Owner.ActionQueue.Count;

	public void ClearQueue()
		=> this.Owner.ActionQueue.clear();

	public void QueueDelay(uint ms)
		=> this.Owner.ActionQueue.add(new PauseAction(ms));

	public void QueueAction(Closure func, params DynValue[] arguments)
		=> this.Owner.ActionQueue.add(new CallbackAction(DynValue.NewClosure(func), arguments));
	public void QueueAction(CallbackFunction func, params DynValue[] arguments)
		=> this.Owner.ActionQueue.add(new CallbackAction(DynValue.NewCallback(func), arguments));

	#endregion

	#region Clipboard

	[AllowNull]
	public static string Clipboard {
		get => ImGui.GetClipboardText() ?? string.Empty;
		set => ImGui.SetClipboardText(value ?? string.Empty);
	}

	#endregion

	// TODO ImGui toasts?

	#region Metamethods

	[MoonSharpUserDataMetamethod("__tostring")]
	public override string ToString()
		=> $"Script[{this.Owner.PrettyName}]";

	[MoonSharpUserDataMetamethod("__call")]
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Lua __call invocations pass target object as first parameter")]
	public void RegisterCallbackFunction(DynValue self, DynValue func) {
		if (this.Owner.SetCallback(func)) {
			this.Log($"Registered on-execute function [{ApiBase.ToUsefulString(func, true)}]", "CALLBACK");
		}
		else {
			this.Log(
				$"Got a {func.Type}{(func.Type is DataType.UserData ? ("(" + (func.UserData.Object is null ? "static unknown" : func.UserData.Object.GetType().FullName) + ")") : "")} instead of a function",
				"CALLBACK"
			);
			throw new ScriptRuntimeException("The provided value to Script() must be function to run when the script is called");
		}
	}

	#endregion
}
