namespace PrincessRTFM.WoLua;

using System.Collections.Concurrent;

using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui.Dtr;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Game;
using PrincessRTFM.WoLua.Lua;

using XivCommon;

internal class Service {
	public static ConcurrentDictionary<string, ScriptContainer> Scripts { get; } = new();

	[PluginService] public static Plugin Plugin { get; private set; } = null!;
	[PluginService] public static PluginConfiguration Configuration { get; private set; } = null!;
	[PluginService] public static XivCommonBase Common { get; private set; } = null!;

	[PluginService] public static DalamudPluginInterface Interface { get; private set; } = null!;
	[PluginService] public static IChatGui ChatGui { get; private set; } = null!;
	[PluginService] public static IClientState ClientState { get; private set; } = null!;
	[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] public static ICondition Condition { get; private set; } = null!;
	[PluginService] public static IDataManager DataManager { get; private set; } = null!;
	[PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
	[PluginService] public static IFateTable FateTable { get; private set; } = null!;
	[PluginService] public static IFramework Framework { get; private set; } = null!;
	[PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
	[PluginService] public static IGameGui GameGui { get; private set; } = null!;
	[PluginService] public static IGameInteropProvider Interop { get; private set; } = null!;
	[PluginService] public static IGameLifecycle GameLifecycle { get; private set; } = null!;
	[PluginService] public static IJobGauges JobGauges { get; private set; } = null!;
	[PluginService] public static IObjectTable Objects { get; private set; } = null!;
	[PluginService] public static IPartyList Party { get; private set; } = null!;
	[PluginService] public static IPluginLog Log { get; private set; } = null!;
	[PluginService] public static ISigScanner Scanner { get; private set; } = null!;
	[PluginService] public static ITargetManager Targets { get; private set; } = null!;
	[PluginService] public static IToastGui Toast { get; private set; } = null!;

	public static PlaySound Sounds { get; internal set; } = null!;
	public static Hooks Hooks { get; internal set; } = null!;
	public static DtrBarEntry StatusLine { get; private set; } = null!;

	public Service() {
		StatusLine = DtrBar.Get($"{Plugin.Name} status", StatusText.Initialising);
		StatusLine.Tooltip = StatusText.TooltipInitialising;
		StatusLine.OnClick = Plugin.Rescan;
		StatusLine.Shown = true;
	}
}
