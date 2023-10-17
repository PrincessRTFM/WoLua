namespace PrincessRTFM.WoLua.Lua.Api;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Interface.Internal.Notifications;

using ImGuiNET;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Actions;
using PrincessRTFM.WoLua.Lua.Api.Script;
using PrincessRTFM.WoLua.Lua.Docs;

// This API is for all for everything that doesn't relate to the actual game itself.
// It also contains script-specific and per-script functionality, like persistent storage.
[MoonSharpUserData]
public class ScriptApi: ApiBase {
	#region Non-API functionality

	[MoonSharpHidden]
	internal ScriptApi(ScriptContainer source) : base(source) {
		this.Storage = new(source.Engine);
		this.StoragePath = Path.ChangeExtension(Path.Combine(Service.Interface.GetPluginConfigDirectory(), this.Owner.InternalName), "json");
	}

	protected string StoragePath { get; }

	protected override void Dispose(bool disposing) {
		if (this.Disposed)
			return;
		base.Dispose(disposing);

		this.Storage = null!;
	}

	protected internal override void PreInit() {
		this.ReloadStorage();
		this.Owner.Engine.Options.DebugPrint = this.Debug.PrintString;
		this.Owner.Engine.Options.DebugInput = this.Debug.Input;
	}

	#endregion

	#region Sub-API access

	public DebugApi Debug { get; private set; } = null!;

	public KeysApi Keys { get; private set; } = null!;

	#endregion

	#region Storage

	[LuaDoc("The script's persistent storage table")]
	public Table Storage { get; protected set; }

	[LuaDoc("Completely clears the persistent storage table AND deletes the saved file on disk")]
	[return: LuaDoc("false if the disk file couldn't be deleted, otherwise true (whether or not it existed before)")]
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

	[LuaDoc("Save the current contents of the persistent storage to disk")]
	[return: LuaDoc("true if the disk file was written successfully, false if there was an error")]
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

	[LuaDoc("Try to load the saved persistent storage from disk, REPLACING the existing contents")]
	[return: LuaDoc("true on success, false if no disk file was found, nil if an error occurred")]
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

	[LuaDoc("Replace the script's existing persistent storage table with a new one")]
	public void SetStorage([LuaDoc("The table to COPY as the new persistent storage")] Table replacement) {
		if (this.Disposed)
			return;

		this.Log("Replacing script storage", LogTag.ScriptStorage);
		Table store = new(this.Owner.Engine);
		this.Owner.cleanTable(replacement);
		foreach (TablePair item in replacement.Pairs) {
			store[item.Key] = item.Value;
		}
		this.Storage = store;
	}

	#endregion

	#region Common strings

	[LuaDoc(Plugin.Name + "'s core chat command")]
	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
	public string PluginCommand => Plugin.Command;

	[LuaDoc("The INTERNAL name of this script, used to call it. May or may not be the same as the title.")]
	public string Name => this.Owner.InternalName;

	[LuaDoc("The PRETTY name of this script, for display purposes **ONLY**. DO NOT attempt to call the script with this name, as it may not be recognised.")]
	public string Title => this.Owner.PrettyName;

	[LuaDoc("The command to call this script, respecting the \"direct script commands\" setting")]
	public string CallSelfCommand => Service.Configuration.RegisterDirectCommands ? $"/{Service.Configuration.DirectInvocationCommandPrefix}{this.Name}" : this.PluginCommand + " call " + this.Name;

	#endregion

	#region Action queueing

	[LuaDoc("How many actions are currently in this script's action queue")]
	public int QueueSize => this.Owner.ActionQueue.Count;

	[LuaDoc("Clears this script's action queue entirely, but does NOT interrupt existing functions being run")]
	public void ClearQueue() => this.Owner.ActionQueue.clear();

	[LuaDoc("Queues a pause of the given number of milliseconds before following actions are processed")]
	public void QueueDelay(uint milliseconds) => this.Owner.ActionQueue.add(new PauseAction(milliseconds));

	[LuaDoc("Queues the execution of a function with the provided arguments (if any)")]
	public void QueueAction(Closure callback, [AsLuaType(LuaType.Any), Optional] params DynValue[] arguments)
		=> this.Owner.ActionQueue.add(new CallbackAction(DynValue.NewClosure(callback), arguments));

	[SkipDoc("It's a type-only overload of the above")]
	public void QueueAction(CallbackFunction callback, params DynValue[] arguments)
		=> this.Owner.ActionQueue.add(new CallbackAction(DynValue.NewCallback(callback), arguments));

	#endregion

	#region Clipboard

	[AllowNull]
	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
	[LuaDoc("The user's current system clipboard contents")]
	public string Clipboard {
		get => ImGui.GetClipboardText() ?? string.Empty;
		set => ImGui.SetClipboardText(value ?? string.Empty);
	}

	#endregion

	#region JSON

	[LuaDoc("Parses a string containing a JSON object/array and turns it into a lua table")]
	[return: LuaDoc("The resulting lua table, or nil if the JSON wasn't a valid object/array")]
	public Table? ParseJson(string jsonObject) {
		this.Log(jsonObject, LogTag.JsonParse);
		try {
			Table table = JsonTableConverter.JsonToTable(jsonObject, this.Owner.Engine);
			this.Owner.cleanTable(table);
			return table;
		}
		catch (SyntaxErrorException e) {
			this.Log(e.ToString(), LogTag.JsonParse);
			return null;
		}
	}

	[LuaDoc("Serialises a lua table into a JSON object, or an array if all keys are numeric")]
	[return: LuaDoc("The resulting JSON object/array as a string")]
	public string SerialiseJson(Table content) {
		this.Owner.cleanTable(content);
		string json = JsonTableConverter.TableToJson(content);
		this.Log(json, LogTag.JsonDump);
		return json;
	}

	[LuaDoc("Serialises a lua table into a JSON object, or an array if all keys are numeric")]
	[return: LuaDoc("The resulting JSON object/array as a string")]
	public string SerializeJson(Table content) => this.SerialiseJson(content); // american spelling

	#endregion

	#region Non-game Dalamud access

	[Obsolete("Use the DalamudApi version instead")]
	[return: LuaDoc("Whether or not a plugin with the given INTERNAL name is installed and loaded")]
	public bool HasPlugin(string pluginName) {
		this.DeprecationWarning("Game.Dalamud.HasPlugin(string)");
		return this.Owner.GameApi.Dalamud.HasPlugin(pluginName);
	}

	private void showNotification(string content, NotificationType type, double durationModifier = 1) {
		int initialDuration = 5000;
		initialDuration += 50 * content.Length;
		initialDuration += 25 * content.Split(' ', '\n', '\r', '\t').Length;
		initialDuration += 10 * content.Count(char.IsPunctuation);
		uint duration = (uint)Math.Abs(Math.Ceiling(initialDuration * durationModifier));
		this.Log($"Displaying notification (type {type}) of {content.Length:N} chars for {duration:N}ms ({initialDuration:D}ms x {durationModifier:F2})", LogTag.DebugMessage);
		Service.Interface.UiBuilder.AddNotification(content, $"{Plugin.Name}: {this.Title}", type, duration);
	}

	[LuaDoc("Displays a Dalamud popup debug notification in the lower right corner **IF** the script has debug mode enabled")]
	public void NotifyDebug(string content) {
		if (this.Debug.Enabled)
			this.showNotification(content, NotificationType.None);
	}
	[LuaDoc("Displays a Dalamud popup informational notification in the lower right corner")]
	public void NotifyInfo(string content) => this.showNotification(content, NotificationType.Info);
	[LuaDoc("Displays a Dalamud popup success notification in the lower right corner")]
	public void NotifySuccess(string content) => this.showNotification(content, NotificationType.Success);
	[LuaDoc("Displays a Dalamud popup warning notification in the lower right corner")]
	public void NotifyWarning(string content) => this.showNotification(content, NotificationType.Warning, 1.1);
	[LuaDoc("Displays a Dalamud popup error notification in the lower right corner")]
	public void NotifyError(string content) => this.showNotification(content, NotificationType.Error, 1.2);

	#endregion

	#region Metamethods

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => $"Script[{this.Owner.PrettyName}]";

	[MoonSharpHidden]
	[MoonSharpUserDataMetamethod(Metamethod.FunctionCall)]
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Lua __call invocations pass target object as first parameter")]
	public void RegisterCallbackFunction(DynValue self, [AsLuaType(LuaType.Function)] DynValue func) {
		if (this.Owner.SetCallback(func)) {
			this.Log($"Registered on-execute function [{ToUsefulString(func, true)}]", LogTag.CallbackRegistration);
		}
		else {
			string descriptor = func.Type.ToString() + (func.Type is DataType.UserData ? ("(" + (func.UserData.Object is null ? "static unknown" : func.UserData.Object.GetType().FullName) + ")") : "");
			this.Log($"Got a {descriptor} instead of a function", LogTag.CallbackRegistration);
			throw new ScriptRuntimeException("The provided value to Script() must be function to run when the script is called");
		}
	}

	#endregion
}
