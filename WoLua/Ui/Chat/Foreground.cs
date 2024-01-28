using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace PrincessRTFM.WoLua.Ui.Chat;

public static class Foreground {
	public static readonly UIForegroundPayload
		Reset = new(0),
		Self = new(48),
		Script = new(37),
		Normal = new(43),
		Debug = new(3),
		Error = new(15);
}
