namespace PrincessRTFM.WoLua.Constants;

using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

using PrincessRTFM.WoLua.Lua;

public static class StatusText {
	public static string IconPlugin { get; } = SeIconChar.CrossWorld.ToIconString();
	public static string IconInitialising { get; } = SeIconChar.Experience.ToIconString();
	public static string IconLoadingScripts { get; } = SeIconChar.Clock.ToIconString();
	public static string IconErrored { get; } = SeIconChar.Cross.ToIconString();
	public static string IconDisposing { get; } = SeIconChar.ExperienceFilled.ToIconString();

	public static SeString Initialising { get; } = $"{IconPlugin} {IconInitialising}";
	public static SeString LoadingScripts { get; } = $"{IconPlugin} {IconLoadingScripts}";
	public static SeString Scripts {
		get {
			IEnumerable<ScriptContainer> scripts = Service.Scripts.Values;
			int total = Service.Scripts.Count;
			int loaded = scripts.Where(c => c.Active).Count();
			int failed = total - loaded;
#if DEBUG
			return $"{IconPlugin}{loaded}/{total} {IconErrored}{failed}";
#else
			return loaded == total
				? $"{IconPlugin}{loaded}"
				: $"{IconPlugin}{loaded}/{total} {IconErrored}{failed}";
#endif
		}
	}
	public static SeString Disposing { get; } = $"{IconPlugin} {IconDisposing}";
}
