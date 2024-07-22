using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace PrincessRTFM.WoLua.Constants;

public static class StatusText {
	public static string IconScripts { get; } = SeIconChar.CrossWorld.ToIconString();
	public static string IconInitialising { get; } = SeIconChar.Experience.ToIconString();
	public static string IconLoadingScripts { get; } = SeIconChar.Clock.ToIconString();
	public static string IconErrored { get; } = SeIconChar.Cross.ToIconString();
	public static string IconDisposing { get; } = SeIconChar.ExperienceFilled.ToIconString();

	public static SeString Initialising { get; } = $"{IconScripts} {IconInitialising}";
	public static SeString LoadingScripts { get; } = $"{IconScripts} {IconLoadingScripts}";
	public static SeString Scripts {
		get {
			int total = Service.ScriptManager.TotalScripts;
			int loaded = Service.ScriptManager.WorkingScripts;
			int failed = total - loaded;
			return
#if !DEBUG
				loaded == total ? $"{IconScripts}{loaded}" :
#endif
				$"{IconScripts}{loaded}/{total} {IconErrored}{failed}";
		}
	}
	public static SeString Disposing { get; } = $"{IconScripts} {IconDisposing}";

	public static SeString TooltipInitialising { get; } = $"{Plugin.Name} initialising, please wait...";
	public static SeString TooltipLoadingScripts { get; } = "Loading scripts...";
	public static SeString TooltipLoaded {
		get {
			int totalCount = Service.ScriptManager.TotalScripts;
			string totalNoun = totalCount == 1 ? "script" : "scripts";
			int loadedCount = Service.ScriptManager.WorkingScripts;
			string loadedNoun = loadedCount == 1 ? "script" : "scripts";
			int failedCount = totalCount - loadedCount;
			string failedNoun = failedCount == 1 ? "script" : "scripts";
			return $"{totalCount} {totalNoun} found.\n{loadedCount} {loadedNoun} loaded successfully.\n{failedCount} {failedNoun} failed to load.\nClick to reload all scripts.";
		}
	}
	public static SeString TooltipDisposing { get; } = "Shutting down...";
}
