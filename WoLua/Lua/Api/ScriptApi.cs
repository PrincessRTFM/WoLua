namespace PrincessRTFM.WoLua.Lua.Api;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using ImGuiNET;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Actions;
using PrincessRTFM.WoLua.Lua.Api.Script;

// This API is for all for everything that doesn't relate to the actual game itself.
// It also contains script-specific and per-script functionality, like persistent storage.
[MoonSharpUserData]
public class ScriptApi: ApiBase {
	#region Non-API functionality

	[MoonSharpHidden]
	internal ScriptApi(ScriptContainer source) : base(source) {
		this.Storage = new(source.Engine);
		this.StoragePath = Path.ChangeExtension(Path.Combine(Service.Interface.GetPluginConfigDirectory(), this.Owner.InternalName), "json");
		this.Debug = new(this.Owner);
		this.Keys = new(this.Owner);
	}

	protected string StoragePath { get; }

	protected override void Dispose(bool disposing) {
		if (this.Disposed)
			return;
		base.Dispose(disposing);

		this.Storage = null!;
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
		this.Log($"Deleting {this.StoragePath}", LogTag.ScriptStorage);
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

		this.Owner.cleanTable(this.Storage);
		this.Log($"Writing to {this.StoragePath}", LogTag.ScriptStorage);
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

		this.Log($"Loading from {this.StoragePath}", LogTag.ScriptStorage);
		try {
			string json = File.ReadAllText(this.StoragePath);
			Table loaded = JsonTableConverter.JsonToTable(json, this.Owner.Engine);
			if (loaded is null)
				return null;
			this.Owner.cleanTable(loaded);
			this.Storage = loaded;
			return true;
		}
		catch (FileNotFoundException) {
			this.Log("No disk storage found", LogTag.ScriptStorage);
			return false;
		}
		catch (SyntaxErrorException err) {
			Service.Plugin.Error($"Invalid JSON disk storage for {this.Owner.PrettyName}", err);
			return null;
		}
		catch (Exception err) {
			Service.Plugin.Error($"Failed to load storage for {this.Owner.PrettyName}", err);
			return null;
		}
	}

	public void SetStorage(Table update) {
		if (this.Disposed)
			return;

		this.Log("Replacing script storage", LogTag.ScriptStorage);
		Table store = new(this.Owner.Engine);
		this.Owner.cleanTable(update);
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

	#region JSON

	public DynValue ParseJson(string content) {
		this.Log(content, LogTag.JsonParse);
		try {
			Table table = JsonTableConverter.JsonToTable(content, this.Owner.Engine);
			this.Owner.cleanTable(table);
			return DynValue.NewTable(table);
		}
		catch (SyntaxErrorException e) {
			this.Log(e.ToString(), LogTag.JsonParse);
			return DynValue.Nil;
		}
	}
	public string SerialiseJson(Table content) {
		this.Owner.cleanTable(content);
		string json = JsonTableConverter.TableToJson(content);
		this.Log(json, LogTag.JsonDump);
		return json;
	}
	public string SerializeJson(Table content) // american spelling
		=> this.SerialiseJson(content);

	#endregion

	// TODO check if a given plugin is installed
	// TODO ImGui toasts?

	#region Metamethods

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => $"Script[{this.Owner.PrettyName}]";

	[MoonSharpUserDataMetamethod(Metamethod.FunctionCall)]
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Lua __call invocations pass target object as first parameter")]
	public void RegisterCallbackFunction(DynValue self, DynValue func) {
		if (this.Owner.SetCallback(func)) {
			this.Log($"Registered on-execute function [{ApiBase.ToUsefulString(func, true)}]", LogTag.CallbackRegistration);
		}
		else {
			string descriptor = func.Type.ToString() + (func.Type is DataType.UserData ? ("(" + (func.UserData.Object is null ? "static unknown" : func.UserData.Object.GetType().FullName) + ")") : "");
			this.Log($"Got a {descriptor} instead of a function", LogTag.CallbackRegistration);
			throw new ScriptRuntimeException("The provided value to Script() must be function to run when the script is called");
		}
	}

	#endregion
}
