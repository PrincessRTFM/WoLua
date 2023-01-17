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

using PrincessRTFM.WoLua.Lua;

using XivCommon;

internal class Service {
	public static readonly Dictionary<string, ScriptContainer> Scripts = new();

	[PluginService] public static Plugin Plugin { get; private set; } = null!;
	[PluginService] public static PluginConfiguration Configuration { get; private set; } = null!;
	[PluginService] public static XivCommonBase Common { get; private set; } = null!;

	[PluginService] public static Framework Framework { get; private set; } = null!;
	[PluginService] public static ChatGui Chat { get; private set; } = null!;
	[PluginService] public static GameGui Gui { get; private set; } = null!;
	[PluginService] public static ToastGui Toast { get; private set; } = null!;
	[PluginService] public static DalamudPluginInterface Interface { get; private set; } = null!;
	[PluginService] public static SigScanner Scanner { get; private set; } = null!;
	[PluginService] public static CommandManager CommandManager { get; private set; } = null!;
	[PluginService] public static ClientState Client { get; private set; } = null!;
	[PluginService] public static Condition Conditions { get; private set; } = null!;
	[PluginService] public static TargetManager Targets { get; private set; } = null!;
	[PluginService] public static DataManager DataManager { get; private set; } = null!;
	[PluginService] public static PartyList Party { get; private set; } = null!;
	[PluginService] public static ObjectTable Objects { get; private set; } = null!;
}
