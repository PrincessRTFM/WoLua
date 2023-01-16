namespace PrincessRTFM.WoLua.Ui;

using Dalamud.Interface.Windowing;

using ImGuiNET;

internal abstract class BaseWindow: Window {
	protected BaseWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow) {
		this.RespectCloseHotkey = true;
		this.IsOpen = false;
	}



	protected static void Centred(string text, uint spacing = 1) {
		float width = ImGui.CalcTextSize(text).X;
		float offset = (ImGui.GetContentRegionAvail().X / 2) - (width / 2);
		ImGui.SetCursorPosX(offset);
		Textline(text, spacing);
	}

	protected static void Textline(string? text = null, uint spacing = 1) {
		if (spacing > 0) {
			for (uint i = 0; i < spacing; ++i)
				ImGui.Spacing();
		}
		ImGui.TextUnformatted(text ?? string.Empty);
		if (spacing > 0) {
			for (uint i = 0; i < spacing; ++i)
				ImGui.Spacing();
		}
	}

	protected static void Separator(uint spacing = 1) {
		if (spacing > 0) {
			for (uint i = 0; i < spacing; ++i)
				ImGui.Spacing();
		}
		ImGui.Separator();
		if (spacing > 0) {
			for (uint i = 0; i < spacing; ++i)
				ImGui.Spacing();
		}
	}

	protected static bool Section(string title)
		=> ImGui.CollapsingHeader(title, ImGuiTreeNodeFlags.None);
}
