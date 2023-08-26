namespace PrincessRTFM.WoLua.Lua.Api.Game;

using System;
using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Logging;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

[MoonSharpUserData]
public class PlayerApi: ApiBase {

	[MoonSharpHidden]
	internal PlayerApi(ScriptContainer source) : base(source) {
		this.Party = new(this.Owner);
	}

	public bool Loaded
		=> !this.Disposed
		&& Service.Client.LocalPlayer is not null
		&& Service.Client.LocalContentId is not 0;
	public static implicit operator bool(PlayerApi? player) => player?.Loaded ?? false;

	public ulong? CharacterId
		=> this.Loaded
			? Service.Client.LocalContentId
		: null;

	public EntityWrapper Entity => new(this ? Service.Client.LocalPlayer : null);

	public MountData Mount => this.Entity.Mount;

	#region Name

	public string? Name
		=> this.Loaded
			? Service.Client.LocalPlayer!.Name!.TextValue
		: null;

	public string? Firstname
		=> this.Loaded
			? this.Name!.Split(' ')[0]
		: null;

	public string? Lastname
		=> this.Loaded
			? this.Name!.Split(' ')[1]
		: null;

	#endregion

	#region Location

	public uint? MapZone
		=> this.Loaded && Service.Client.TerritoryType > 0
			? Service.Client.TerritoryType
		: null;

	#endregion

	#region Condition flags

	public bool? InCombat
		=> this.Loaded
			? Service.Conditions[ConditionFlag.InCombat]
			|| Service.Client.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.InCombat)
		: null;

	public bool Mounted => this.Mount.Active;

	public bool? Crafting
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Crafting]
			|| Service.Conditions[ConditionFlag.Crafting40]
			|| Service.Conditions[ConditionFlag.PreparingToCraft]
		: null;

	public bool? Gathering
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Gathering]
			|| Service.Conditions[ConditionFlag.Gathering42]
		: null;

	public bool? Fishing
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Fishing]
		: null;

	public bool? Performing
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Performing]
		: null;

	public bool? Casting
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Casting]
			|| Service.Conditions[ConditionFlag.Casting87]
			|| Service.Client.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.IsCasting)
		: null;

	public bool? InCutscene
		=> this.Loaded
			? Service.Conditions[ConditionFlag.WatchingCutscene]
			|| Service.Conditions[ConditionFlag.WatchingCutscene78]
			|| Service.Conditions[ConditionFlag.OccupiedInCutSceneEvent]
			|| Service.Conditions[ConditionFlag.BetweenAreas]
			|| Service.Conditions[ConditionFlag.BetweenAreas51]
		: null;

	public bool? Trading
		=> this.Loaded
			? Service.Conditions[ConditionFlag.TradeOpen]
		: null;

	public bool? Flying
		=> this.Loaded
			? Service.Conditions[ConditionFlag.InFlight]
		: null;

	public bool? Swimming
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Swimming]
		: null;

	public bool? Diving
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Diving]
		: null;

	public bool? Jumping
		=> this.Loaded
			? Service.Conditions[ConditionFlag.Jumping]
			|| Service.Conditions[ConditionFlag.Jumping61]
		: null;

	public bool? InDuty
		=> this.Loaded
			? Service.Conditions[ConditionFlag.BoundByDuty]
			|| Service.Conditions[ConditionFlag.BoundByDuty56]
			|| Service.Conditions[ConditionFlag.BoundByDuty95]
			|| Service.Conditions[ConditionFlag.BoundToDuty97]
		: null;

	public bool? UsingFashionAccessory
		=> this.Loaded
			? Service.Conditions[ConditionFlag.UsingParasol]
		: null;

	public bool? WeaponDrawn
		=> this.Loaded
			? Service.Client.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.WeaponOut)
		: null;

	public static unsafe bool Moving
		=> AgentMap.Instance() is not null && AgentMap.Instance()->IsPlayerMoving > 0;

	#endregion

	#region Party/alliance

	public PartyApi Party { get; private set; }

	#endregion

	#region Targets

	public EntityWrapper Target => new(this.Loaded ? Service.Targets.Target : null);
	public bool? HasTarget => this.Loaded ? this.Target : null;

	public EntityWrapper SoftTarget => new(this.Loaded ? Service.Targets.SoftTarget : null);
	public bool? HasSoftTarget => this.Loaded ? this.SoftTarget : null;

	public EntityWrapper FocusTarget => new(this.Loaded ? Service.Targets.FocusTarget : null);
	public bool? HasFocusTarget => this.Loaded ? this.FocusTarget : null;

	public EntityWrapper FieldMouseOverTarget => new(this.Loaded ? Service.Targets.MouseOverTarget : null);
	public bool? HasFieldMouseoverTarget => this.Loaded ? this.MouseOverTarget : null;

	public EntityWrapper UiMouseOverTarget => new(this.Loaded ? Service.Hooks.UITarget : null);
	public bool? HasUiMouseoverTarget => this.Loaded ? this.MouseOverTarget : null;

	public EntityWrapper MouseOverTarget {
		get {
			if (!this.Loaded)
				return new(null);
			EntityWrapper found = this.UiMouseOverTarget;
			if (!found)
				found = this.FieldMouseOverTarget;
			return found;
		}
	}
	public bool? HasMouseoverTarget => this.Loaded ? this.MouseOverTarget : null;

	#endregion

	#region Emotes

	private static bool emotesLoaded = false;
	private static readonly Dictionary<string, uint> emoteUnlocks = new();

	[MoonSharpHidden]
	public static void InitialiseEmotes() {
		if (emotesLoaded)
			return;
		emotesLoaded = true;
		PluginLog.Information($"[{LogTag.Emotes}] Initialising API data");

		ExcelSheet<Emote> emotes = Service.DataManager.GameData.GetExcelSheet<Emote>()!;
		try {
			uint max = emotes.RowCount;
			PluginLog.Information($"[{LogTag.Emotes}] Indexing {max:N0} emotes...");
			for (uint i = 0; i < max; ++i) {
				Emote? emote = emotes.GetRow(i);
				if (emote is not null) {
					string[] commands = (new string?[] {
						emote.Name.RawString,
						emote.TextCommand.Value?.Command?.ToString(),
						emote.TextCommand.Value?.ShortCommand?.ToString(),
						emote.TextCommand.Value?.Alias?.ToString(),
						emote.TextCommand.Value?.ShortAlias?.ToString(),
					})
						.Where(s => !string.IsNullOrWhiteSpace(s))
						.Cast<string>()
						.Select(s => s.StartsWith('/') ? s[1..] : s)
						.ToArray();
					foreach (string command in commands)
						emoteUnlocks[command] = emote.UnlockLink;
				}
			}
			PluginLog.Information($"[{LogTag.Emotes}] Cached {emoteUnlocks.Count:N0} emote names");
		}
		catch (Exception e) {
			Service.Plugin.Error("Unable to load Emote sheet, cannot check emote unlock state!", e);
		}

	}

	public unsafe bool? HasEmote(string name) {
		if (!this.Loaded)
			return null;

		string emote = name.StartsWith('/') ? name[1..] : name;
		this.Log($"Checking whether '{emote}' is unlocked", LogTag.Emotes);
		if (!emoteUnlocks.TryGetValue(emote, out uint unlockLink)) {
			this.Log("Can't find unlock link in cached map", LogTag.Emotes);
			return null;
		}
		UIState* uiState = UIState.Instance();
		if (uiState is null || (IntPtr)uiState == IntPtr.Zero) {
			this.Log("UIState is null", LogTag.Emotes);
			return null;
		}
		bool has = uiState->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink, 1);
		this.Log($"UIState reports emote is {(has ? "un" : "")}locked", LogTag.Emotes);
		return has;
	}

	#endregion

	// TODO status effects?

	#region Metamethods

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString()
		=> this.Loaded
			? $"{this.Name}@{this.Entity.HomeWorld}"
		: string.Empty;

	#endregion
}
