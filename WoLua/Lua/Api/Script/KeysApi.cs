namespace PrincessRTFM.WoLua.Lua.Api.Script;

using ImGuiNET;

using MoonSharp.Interpreter;

[MoonSharpUserData]
public class KeysApi: ApiBase { // TODO luadoc all of this
	public KeysApi(ScriptContainer source) : base(source) { }

	public static bool Control
		=> ImGui.IsKeyDown(ImGuiKey.ModCtrl);
	public static bool Ctrl
		=> Control;

	public static bool Alt
		=> ImGui.IsKeyDown(ImGuiKey.ModAlt);

	public static bool Shift
		=> ImGui.IsKeyDown(ImGuiKey.ModShift);

}
