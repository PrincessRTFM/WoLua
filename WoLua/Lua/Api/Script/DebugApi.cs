using System.Diagnostics.CodeAnalysis;
using System.Linq;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Docs;

namespace PrincessRTFM.WoLua.Lua.Api.Script;

[MoonSharpUserData]
public class DebugApi: ApiBase {
	#region Non-API functionality

	[MoonSharpHidden]
	internal DebugApi(ScriptContainer source) : base(source) { }

	#endregion

	// Debug builds force scripts to run with debug mode enabled, because if you're running a debug build it's assumed you're debugging things
	[LuaDoc("Whether or not this script is running in debug mode. Debug output will only be produced if this is enabled.",
		"In debug builds of " + Plugin.Name + ", this is always on and cannot be disabled. Otherwise, you can change it at any time to control when debugging output will be written.",
		"You can check `.PluginDebugBuild` to see if your script is running in a forced-debug environment.")]
#if DEBUG
	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Retain compatibility in the rest of the codebase with non-debug builds")]
	public bool Enabled {
		get => true;
		set => _ = value;
	}
#else
	public bool Enabled { get; set; } = false;
#endif

	[LuaDoc("Whether or not you are running a debug build of " + Plugin.Name + " itself.",
		"This value is constant and determined by " + Plugin.Name + " when it is compiled.")]
#if DEBUG
	public static bool PluginDebugBuild => true;
#else
	public static bool PluginDebugBuild => false;
#endif

	[LuaDoc("Prints the given (singular, string) message to the plugin debug log, accessible via dalamud's `/xldebug` command.")]
	public void PrintString(string message) {
		if (this.Disposed)
			return;

		this.Log(message, LogTag.DebugMessage);
	}

	[MoonSharpUserDataMetamethod(Metamethod.FunctionCall)]
	[LuaDoc("Prints all given values to the plugin debug log, accessible via dalamud's `/xldebug` command.",
		"Special values will be converted to the most useful string form available, such as tables rendering into JSON.")]
	public void Print(params DynValue[] values) {
		if (this.Disposed)
			return;

		this.PrintString(string.Join(" ", values.Select(dv => ToUsefulString(dv))));
	}

	[MoonSharpHidden]
	// This isn't exposed to the scripts themselves because it's basically a nearly-nop filler, but it's still part of the debug API so it goes in here
	public string Input(string prompt) {
		if (!this.Disposed)
			this.Log(prompt, LogTag.ScriptInput);

		return string.Empty;
	}

	[LuaDoc("Dumps the script's CURRENT storage contents to the plugin debug log as JSON, accessible via dalamud's `/xldebug` command.",
		"This is not necessarily the value stored on disk, as the script may have modified it but not yet called the save/reload method.")]
	public void DumpStorage() {
		if (this.Disposed)
			return;

		this.Log(this.Owner.ScriptApi.Storage.TableToJson(), LogTag.ScriptStorage);
	}

	[LuaDoc("Dumps each individual value as a separate in the plugin debug log, accessible via dalamud's `/xldebug` command.",
		"Special values will be converted to the most useful string form available, such as tables rendering into JSON.",
		"Each value will be prefixed with its type.")]
	public void Dump(params DynValue[] values) {
		if (this.Disposed)
			return;

		this.PrintString($"BEGIN VALUE DUMP: {values.Length}");
		int size = values.Length.ToString().Length;
		for (int i = 0; i < values.Length; ++i) {
			DynValue v = values[i];
			this.PrintString($"{(i + 1).ToString().PadLeft(size)}: {ToUsefulString(v, true)}");
		}
		this.PrintString("END VALUE DUMP");
	}

}
