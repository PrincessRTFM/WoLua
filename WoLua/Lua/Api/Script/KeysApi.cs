namespace PrincessRTFM.WoLua.Lua.Api.Script;

using ImGuiNET;

public class KeysApi: ApiBase {
	public KeysApi(ScriptContainer source) : base(source, "KEY") { }

	public static bool Control
		=> ImGui.IsKeyDown(ImGuiKey.ModCtrl);
	public static bool Ctrl
		=> Control;

	public static bool Alt
		=> ImGui.IsKeyDown(ImGuiKey.ModAlt);

	public static bool Shift
		=> ImGui.IsKeyDown(ImGuiKey.ModShift);

}
