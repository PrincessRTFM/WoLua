namespace PrincessRTFM.WoLua;

using System.Collections.Generic;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
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

	[PluginService] public static Framework Framework { get; private set; } = null!;
	[PluginService] public static ChatGui Chat { get; private set; } = null!;
	[PluginService] public static IGameGui Gui { get; private set; } = null!;
	[PluginService] public static ToastGui Toast { get; private set; } = null!;
	[PluginService] public static DalamudPluginInterface Interface { get; private set; } = null!;
	[PluginService] public static ISigScanner Scanner { get; private set; } = null!;
	[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] public static IClientState Client { get; private set; } = null!;
	[PluginService] public static Condition Conditions { get; private set; } = null!;
	[PluginService] public static ITargetManager Targets { get; private set; } = null!;
	[PluginService] public static IDataManager DataManager { get; private set; } = null!;
	[PluginService] public static IPartyList Party { get; private set; } = null!;
	[PluginService] public static IObjectTable Objects { get; private set; } = null!;

	public static PlaySound Sounds { get; internal set; } = null!;
	public static Hooks Hooks { get; internal set; } = null!;
}
