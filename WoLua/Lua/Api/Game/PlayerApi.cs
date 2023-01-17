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

using CSChar = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class PlayerApi: ApiBase {
	public const string TAG = "PLAYER";

	[MoonSharpHidden]
	internal PlayerApi(ScriptContainer source) : base(source, TAG) { }

	public bool Loaded
		=> !this.Disposed
		&& Service.Client.LocalPlayer is not null
		&& Service.Client.LocalContentId is not 0;

	public ulong? CharacterId
		=> this.Loaded
			? Service.Client.LocalContentId
		: null;

	#region Details

	public string? Name
		=> this.Loaded
			? Service.Client.LocalPlayer!.Name!.TextValue
		: null;

	public byte? Level
		=> this.Loaded
			? Service.Client.LocalPlayer!.Level
		: null;

	public JobData Job
		=> new(
			this.Loaded ? Service.Client.LocalPlayer!.ClassJob.Id : 0,
			this.Loaded ? Service.Client.LocalPlayer!.ClassJob!.GameData!.Name!.ToString() : null,
			this.Loaded ? Service.Client.LocalPlayer!.ClassJob!.GameData!.Abbreviation!.ToString() : null
		);

	#endregion

	#region Stats

	public uint? Hp
		=> this.Loaded && (this.MaxHp ?? 0) > 0
			? Service.Client.LocalPlayer!.CurrentHp
		: null;
	public uint? MaxHp
		=> this.Loaded
			? Service.Client.LocalPlayer!.MaxHp
		: null;

	public uint? Mp
		=> this.Loaded && (this.MaxMp ?? 0) > 0
			? Service.Client.LocalPlayer!.CurrentMp
		: null;
	public uint? MaxMp
		=> this.Loaded
			? Service.Client.LocalPlayer!.MaxMp
		: null;

	public uint? Gp
		=> this.Loaded && (this.MaxGp ?? 0) > 0
			? Service.Client.LocalPlayer!.CurrentGp
		: null;
	public uint? MaxGp
		=> this.Loaded
			? Service.Client.LocalPlayer!.MaxGp
		: null;

	public uint? Cp
		=> this.Loaded && (this.MaxCp ?? 0) > 0
			? Service.Client.LocalPlayer!.CurrentCp
		: null;
	public uint? MaxCp
		=> this.Loaded
			? Service.Client.LocalPlayer!.MaxCp
		: null;

	#endregion

	#region Location

	public uint? MapZone
		=> this.Loaded && Service.Client.TerritoryType > 0
			? Service.Client.TerritoryType
		: null;

	// TODO position?

	#endregion

	#region Condition flags

	public bool? InCombat
		=> this.Loaded
			? Service.Conditions[ConditionFlag.InCombat]
			|| Service.Client.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.InCombat)
		: null;

	public unsafe bool? Mounted {
		get {
			if (!this.Loaded)
				return null;
			CSChar* player = (CSChar*)Service.Client.LocalPlayer!.Address;
			if (player is null)
				return null;
			return player->IsMounted();
		}
	}

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

	protected internal static unsafe bool Moving
		=> AgentMap.Instance() is not null && AgentMap.Instance()->IsPlayerMoving > 0;

	#endregion

	#region Party/alliance

	public int? PartyMemberCount
		=> this.Loaded
			? Service.Party.Length
		: null;

	public bool? InAlliance
		=> this.Loaded
			? Service.Party.IsAlliance
		: null;

	public bool? InParty
		=> this.Loaded
			? Service.Party.Length > 0
		: null;

	// TODO party/aliance member details?

	#endregion

	#region Targets

	public bool? HasTarget
		=> this.Loaded
			? Service.Targets.Target is not null
		: null;

	public bool? HasFocusTarget
		=> this.Loaded
			? Service.Targets.FocusTarget is not null
		: null;

	public bool? HasMouseoverTarget
		=> this.Loaded
			? Service.Targets.MouseOverTarget is not null
		: null;

	public bool? HasSoftTarget
		=> this.Loaded
			? Service.Targets.SoftTarget is not null
		: null;

	// TODO target type, target details?

	#endregion

	#region Worlds

	public unsafe ushort? HomeWorldId {
		get {
			if (!this.Loaded)
				return null;
			CSChar* player = (CSChar*)Service.Client.LocalPlayer!.Address;
			if (player is null)
				return null;
			return player->HomeWorld;
		}
	}

	public unsafe ushort? CurrentWorldId {
		get {
			if (!this.Loaded)
				return null;
			CSChar* player = (CSChar*)Service.Client.LocalPlayer!.Address;
			if (player is null)
				return null;
			return player->CurrentWorld;
		}
	}

	public string? HomeWorld {
		get {
			ushort? id = this.HomeWorldId;
			if (id is null)
				return null;
			World? world = Service.DataManager.GetExcelSheet<World>()!.GetRow(id.Value);
			if (world is null)
				return null;
			return world.Name;
		}
	}

	public string? CurrentWorld {
		get {
			ushort? id = this.CurrentWorldId;
			if (id is null)
				return null;
			World? world = Service.DataManager.GetExcelSheet<World>()!.GetRow(id.Value);
			if (world is null)
				return null;
			return world.Name;
		}
	}

	#endregion

	#region Emotes

	private static bool emotesLoaded = false;
	private static readonly Dictionary<string, uint> emoteUnlocks = new();

	[MoonSharpHidden]
	public static void InitialiseEmotes() {
		if (emotesLoaded)
			return;
		emotesLoaded = true;
		PluginLog.Information($"[{TAG}] Initialising API data");

		ExcelSheet<Emote> emotes = Service.DataManager.GameData.GetExcelSheet<Emote>()!;
		try {
			uint max = emotes.RowCount;
			PluginLog.Information($"[{TAG}] Indexing {max:N0} emotes...");
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
			PluginLog.Information($"[{TAG}] Cached {emoteUnlocks.Count:N0} emote names");
		}
		catch (Exception e) {
			Service.Plugin.Error("Unable to load Emote sheet, cannot check emote unlock state!", e);
		}

	}

	public unsafe bool? HasEmote(string name) {
		if (!this.Loaded)
			return null;

		string emote = name.StartsWith('/') ? name[1..] : name;
		this.Log($"Checking whether '{emote}' is unlocked", "EMOTE");
		if (!emoteUnlocks.TryGetValue(emote, out uint unlockLink)) {
			this.Log("Can't find unlock link in cached map", "EMOTE");
			return null;
		}
		UIState* uiState = UIState.Instance();
		if (uiState is null || (IntPtr)uiState == IntPtr.Zero) {
			this.Log("UIState is null", "EMOTE");
			return null;
		}
		bool has = uiState->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink, 1);
		this.Log($"UIState reports emote is {(has ? "un" : "")}locked", "EMOTE");
		return has;
	}

	#endregion

	public unsafe ushort? MountId {
		get {
			if (!this.Loaded)
				return null;
			CSChar* player = (CSChar*)Service.Client.LocalPlayer!.Address;
			if (player is null)
				return null;
			CSChar.MountContainer? mount = player->IsMounted() ? player->Mount : null;
			return mount?.MountId ?? 0;
		}
	}

	// TODO status effects?

	#region Metamethods

	[MoonSharpUserDataMetamethod("__tostring")]
	public override string ToString()
		=> this.Loaded
			? $"{this.Name}@{this.HomeWorld}"
		: string.Empty;

	#endregion
}
