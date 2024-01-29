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

namespace PrincessRTFM.WoLua.Lua.Api;

// This API is for all for everything that doesn't relate to the actual game itself.
// It also contains script-specific and per-script functionality, like persistent storage.
[MoonSharpUserData]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
public class ScriptApi: ApiBase {
	#region Non-API functionality

	[MoonSharpHidden]
	public ScriptApi(ScriptContainer source) : base(source) {
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
		this.Owner.Engine.Options.DebugPrint = this.Debug.PrintString;
		this.Owner.Engine.Options.DebugInput = this.Debug.Input;
	}
	protected internal override void Init() {
		this.Log("Loading script storage from disk", LogTag.ScriptLoader);
		this.ReloadStorage();
	}

	#endregion

	#region Sub-API access

	public DebugApi Debug { get; private set; } = null!;

	public KeysApi Keys { get; private set; } = null!;

	#endregion

	#region Storage

	[LuaDoc("The script's persistent storage table.",
		"This cannot be directly assigned to. If you need to replace the entire table, use the `SetStorage()` method.")]
	public Table Storage { get; protected set; }

	[LuaDoc("Completely clears the persistent storage table, and optionally (by default) deletes the saved file on disk, if it exists.",
		"This differs from calling `SetStorage()` with an empty table in that **`SetStorage()` replaces the table object**, while this method only deletes all values from the existing object.")]
	[return: LuaDoc("`false` if the disk file should have been deleted but couldn't be, otherwise `true` (including if it didn't exist prior)")]
	public bool DeleteStorage([LuaDoc("If this is false, this method will always return true")] bool deleteDiskFile = true) {
		if (this.Disposed)
			return false;

		this.Storage.Clear();

		if (!deleteDiskFile)
			return true;

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

	[LuaDoc("Save the current contents of the persistent storage to disk.",
		$"Script storage persists between script loads, but NOT between PLUGIN loads. If {Plugin.Name} updates or the game restarts, all storage changes not saved to disk are lost.")]
	[return: LuaDoc("`true` if the disk file was written successfully, `false` if there was an error")]
	public bool SaveStorage() {
		if (this.Disposed)
			return false;

		this.Owner.CleanTable(this.Storage);
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

	[LuaDoc("Try to load the saved persistent storage from disk, REPLACING the existing contents.",
		"Note that the table object is replaced entirely, meaning any cached copies will point to the OLD storage.",
		$"You do not need to call this to initialise your script, as {Plugin.Name} does it for you on script load.")]
	[return: LuaDoc("`true` on success, `false` if no disk file was found, `nil` if an error occurred")]
	public bool? ReloadStorage() {
		if (this.Disposed)
			return false;

		this.Log($"Loading from {this.StoragePath}", LogTag.ScriptStorage);
		try {
			string json = File.ReadAllText(this.StoragePath);
			Table loaded = JsonTableConverter.JsonToTable(json, this.Owner.Engine);
			if (loaded is null)
				return null;
			this.Owner.CleanTable(loaded);
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

	[LuaDoc("Replace the script's existing persistent storage table with a new one.",
		"Note that this method replaces the table object entirely, not merely the contents; any cached copies will point to the OLD table and will NOT see any changes.")]
	public void SetStorage([LuaDoc("The table to COPY as the new persistent storage")] Table replacement) {
		if (this.Disposed)
			return;

		this.Log("Replacing script storage", LogTag.ScriptStorage);
		Table store = new(this.Owner.Engine);
		this.Owner.CleanTable(replacement);
		foreach (TablePair item in replacement.Pairs) {
			store[item.Key] = item.Value;
		}
		this.Storage = store;
	}

	#endregion

	#region Common strings

	[LuaDoc($"{Plugin.Name}'s core chat command. This is how everything is done.",
		"Even if direct script commands are registered, you are GUARANTEED to be able to invoke a script using the `call` subcommand of this chat command.")]
	public string PluginCommand => Plugin.Command;

	[LuaDoc("The INTERNAL name of this script, used to call it. May or may not be the same as the title.",
		"Script names are transformed in particular ways in order to produce a guaranteed-valid internal name.")]
	public string Name => this.Owner.InternalName;

	[LuaDoc("The PRETTY name of this script, for display purposes **ONLY**.",
		"DO NOT attempt to call the script with this name, as it may not be recognised, such as if it contains spaces.")]
	public string Title => this.Owner.PrettyName;

	[LuaDoc("The command to call this script, respecting the \"direct script commands\" setting.",
		"This **includes** the leading '/' character, meaning it can be passed directly to the `Game.SendChat()` method in order to call this script again.",
		"Beware of infinite recursion in such cases.")]
	public string CallSelfCommand => Service.Configuration.RegisterDirectCommands ? $"/{Service.Configuration.DirectInvocationCommandPrefix}{this.Name}" : this.PluginCommand + " call " + this.Name;

	#endregion

	#region Action queueing
	// TODO allow examining the contents of the action queue?
	// need to figure out how to handle function-call actions for that
	// maybe allow labelling them?

	[LuaDoc("How many actions are currently in this script's action queue.",
		"The action queue includes queued function calls AND queued delays.",
		"It is not (currently) possible to examine the contents of the queue.")]
	public int QueueSize => this.Owner.ActionQueue.Count;

	[LuaDoc("Clears this script's action queue entirely, but does NOT interrupt existing functions being run.",
		"Any actions not yet started will be discarded, but if the script is already doing something at the same time, it will run that to completion.")]
	public void ClearQueue() => this.Owner.ActionQueue.clear();

	[LuaDoc("Queues a pause of the given number of milliseconds before following actions are processed.",
		"This does NOT pause script execution; the delay is added onto the end of the action queue.",
		"When a delay action is processed, the action queue pauses processing of further actions for the appropriate duration.",
		"This is NOT guaranteed to be millisecond-accurate. Do not attempt to write scripts that depend on millisecond-level timing.")]
	public void QueueDelay(uint milliseconds) => this.Owner.ActionQueue.add(new PauseAction(milliseconds));

	[LuaDoc("Queues the execution of a function with the provided arguments, if any.",
		"When this action is processed by the script's queue, the provided function will be invoked on the game's main thread.",
		"If the function has to do heavy processing (or is poorly written) it may cause game lag.")]
	public void QueueAction(Closure callback, [AsLuaType(LuaType.Any), Optional] params DynValue[] arguments)
		=> this.Owner.ActionQueue.add(new CallbackAction(DynValue.NewClosure(callback), arguments));

	[SkipDoc("It's a type-only overload of the above")]
	public void QueueAction(CallbackFunction callback, params DynValue[] arguments)
		=> this.Owner.ActionQueue.add(new CallbackAction(DynValue.NewCallback(callback), arguments));

	#endregion

	#region Clipboard

	[AllowNull]
	[LuaDoc("The user's current system clipboard contents.",
		"This will never be `nil`; if the contents cannot be read for any reason (such as not being text), an empty string will be returned.",
		"However, you can SET this to nil, in which case the clipboard contents will be set to an empty string.")]
	public string Clipboard {
		get => ImGui.GetClipboardText() ?? string.Empty;
		set => ImGui.SetClipboardText(value ?? string.Empty);
	}

	#endregion

	#region JSON

	[LuaDoc("Parses a string containing a JSON object/array and turns it into a lua table.",
		"This method **does not** support JSON \"primitives\" like strings, numbers, or booleans.",
		"JSON arrays will always produce tables with contiguous numeric keys, objects will depend on the object's own keys.")]
	[return: LuaDoc("The resulting lua table, or nil if the JSON wasn't a valid object/array")]
	public Table? ParseJson(string jsonObject) {
		this.Log(jsonObject, LogTag.JsonParse);
		try {
			Table table = JsonTableConverter.JsonToTable(jsonObject, this.Owner.Engine);
			this.Owner.CleanTable(table);
			return table;
		}
		catch (SyntaxErrorException e) {
			this.Log(e.ToString(), LogTag.JsonParse);
			return null;
		}
	}

	[LuaDoc("Serialises a lua table into a JSON object or array.",
		"If the table's keys are numeric and contiguous, a JSON array will be produced. If not, it will be an object.",
		"This method may also be called as SerializeJson (with a 'z' instead of 's') for the american spelling.")]
	[return: LuaDoc("The resulting JSON object/array as a string")]
	public string SerialiseJson(Table content) {
		this.Owner.CleanTable(content);
		string json = JsonTableConverter.TableToJson(content);
		this.Log(json, LogTag.JsonDump);
		return json;
	}

	[SkipDoc("It's just a spelling variant of the above")]
	public string SerializeJson(Table content) => this.SerialiseJson(content); // american spelling

	#endregion

	#region Non-game Dalamud access

	[Obsolete("Use the DalamudApi version instead")]
	[LuaDoc("Please use the `Game.Dalamud.HasPlugin(string)` method instead. This method will be removed in a future release.")]
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

	[LuaDoc("Displays a Dalamud popup debug notification in the lower right corner.",
		"**Only** displays if the script has debug mode enabled.")]
	public void NotifyDebug(string content) {
		if (this.Debug.Enabled)
			this.showNotification(content, NotificationType.None);
	}
	[LuaDoc("Displays a Dalamud popup informational notification in the lower right corner.")]
	public void NotifyInfo(string content) => this.showNotification(content, NotificationType.Info);
	[LuaDoc("Displays a Dalamud popup success notification in the lower right corner.")]
	public void NotifySuccess(string content) => this.showNotification(content, NotificationType.Success);
	[LuaDoc("Displays a Dalamud popup warning notification in the lower right corner.")]
	public void NotifyWarning(string content) => this.showNotification(content, NotificationType.Warning, 1.1);
	[LuaDoc("Displays a Dalamud popup error notification in the lower right corner.")]
	public void NotifyError(string content) => this.showNotification(content, NotificationType.Error, 1.2);

	#endregion

	#region Metamethods

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => $"Script[{this.Owner.PrettyName}]";

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
