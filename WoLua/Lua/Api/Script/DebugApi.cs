namespace PrincessRTFM.WoLua.Lua.Api.Script;

using System.Linq;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Constants;

[MoonSharpUserData]
public class DebugApi: ApiBase {
	#region Non-API functionality

	[MoonSharpHidden]
	internal DebugApi(ScriptContainer source) : base(source) { }

	#endregion

	// Debug builds force scripts to run with debug mode enabled, because if you're running a debug build it's assumed you're debugging things
#if DEBUG
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Retain compatibility in the rest of the codebase with non-debug builds")]
	public bool Enabled {
		get => true;
		set => _ = value;
	}
#else
	public bool Enabled { get; set; } = false;
#endif

	public void PrintString(string message) {
		if (this.Disposed)
			return;

		this.Log(message, LogTag.DebugMessage);
	}

	[MoonSharpUserDataMetamethod(Metamethod.FunctionCall)]
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

	public void DumpStorage() {
		if (this.Disposed)
			return;

		this.Log(this.Owner.ScriptApi.Storage.TableToJson(), LogTag.ScriptStorage);
	}

	public void Dump(params DynValue[] values) {
		if (this.Disposed)
			return;

		this.Log($"BEGIN VALUE DUMP: {values.Length}", LogTag.DebugMessage);
		int size = values.Length.ToString().Length;
		for (int i = 0; i < values.Length; ++i) {
			DynValue v = values[i];
			this.Log($"{(i + 1).ToString().PadLeft(size)}: {ToUsefulString(v)}", LogTag.DebugMessage);
		}
		this.Log("END VALUE DUMP", LogTag.DebugMessage);
	}

}
