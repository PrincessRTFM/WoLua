namespace PrincessRTFM.WoLua;

using System.Collections.Generic;

using Dalamud.Game;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Config;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.Game.Network;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using PrincessRTFM.WoLua.Game;
using PrincessRTFM.WoLua.Lua;

using XivCommon;

internal class Service {
	public static readonly Dictionary<string, ScriptContainer> Scripts = new();

	[PluginService] public static Plugin Plugin { get; private set; } = null!;
	[PluginService] public static PluginConfiguration Configuration { get; private set; } = null!;
	[PluginService] public static XivCommonBase Common { get; private set; } = null!;
	
	[PluginService] public static DalamudPluginInterface Interface { get; private set; } = null!;

	// [PluginService] public static IAetheryteList AetheryteList { get; private set; } = null!;
	// [PluginService] public static IBuddyList BuddyList { get; private set; } = null!;
	[PluginService] public static ChatGui ChatGui { get; private set; } = null!;
	// [PluginService] public static ChatHandlers ChatHandlers { get; private set; } = null!;
	[PluginService] public static IClientState ClientState { get; private set; } = null!;
	[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] public static Condition Condition { get; private set; } = null!;
	[PluginService] public static IDataManager DataManager { get; private set; } = null!;
	[PluginService] public static IDtrBar DtrBar { get; private set; } = null!; // TODO dtr bar entry with number of loaded scripts
	// [PluginService] public static IDutyState DutyState { get; private set; } = null!;
	[PluginService] public static IFateTable FateTable { get; private set; } = null!;
	// [PluginService] public static FlyTextGui FlyTextGui { get; private set; } = null!;
	[PluginService] public static Framework Framework { get; private set; } = null!;
	[PluginService] public static GameConfig GameConfig { get; private set; } = null!;
	[PluginService] public static IGameGui GameGui { get; private set; } = null!;
	[PluginService] public static IGameLifecycle GameLifecycle { get; private set; } = null!;
	// [PluginService] public static GameNetwork GameNetwork { get; private set; } = null!;
	// [PluginService] public static IGamepadState GamepadState { get; private set; } = null!;
	[PluginService] public static IJobGauges JobGauges { get; private set; } = null!;
	// [PluginService] public static KeyState KeyState { get; private set; } = null!;
	// [PluginService] public static ILibcFunction LibcFunction { get; private set; } = null!;
	[PluginService] public static IObjectTable Objects { get; private set; } = null!;
	// [PluginService] public static PartyFinderGui PartyFinderGui { get; private set; } = null!;
	[PluginService] public static IPartyList Party { get; private set; } = null!;
	[PluginService] public static ISigScanner Scanner { get; private set; } = null!;
	[PluginService] public static ITargetManager Targets { get; private set; } = null!;
	// [PluginService] public static TitleScreenMenu TitleScreenMenu { get; private set; } = null!;
	[PluginService] public static ToastGui Toast { get; private set; } = null!;

	public static PlaySound Sounds { get; internal set; } = null!;
	public static Hooks Hooks { get; internal set; } = null!;
}
