using System;
using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Docs;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
//[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
public class PlayerApi: ApiBase { // TODO luadoc all of this

	[MoonSharpHidden]
	internal PlayerApi(ScriptContainer source) : base(source) { }

	[LuaDoc("Whether or not the player is currently loaded.",
		"If you aren't logged in, this will be `false`.",
		"If this is `false`, then all player properties will be `nil`.")]
	public bool Loaded => !this.Disposed
		&& Service.ClientState.LocalPlayer is not null
		&& Service.ClientState.LocalContentId is not 0;
	public static implicit operator bool(PlayerApi? player) => player?.Loaded ?? false;

	[LuaPlayerDoc("This is the _universally unique_ ID of the character currently logged in.")]
	public ulong? CharacterId => this.Loaded
		? Service.ClientState.LocalContentId
		: null;

	[LuaDoc("This provides an `EntityWrapper` wrapper object around the currently-logged-in player (or around nothing) _at the time of access_.",
		"If you cache this, it may become invalid, such as if the player logs out.",
		"It is recommended that you only cache this in a function-local variable, and not rely on it remaining valid between script invocations, especially if you use the action queue.",
		"This value itself will _never_ be `nil`, but _will_ represent a nonexistent/invalid game entity if the `Loaded` property is `false`.")]
	public EntityWrapper Entity => new(this ? Service.ClientState.LocalPlayer : null);

	[LuaDoc("This provides a `MountData` wrapper object around the currently-logged-in player's mount (or around nothing) _at the time of access_.",
		"If you cache this, it may become invalid, such as if the player logs out, changes mounts, or dismounts entirely.",
		"It is recommended that you only cache this in a function-local variable, and not rely on it remaining valid between script invocations, especially if you use the action queue.",
		"This value itself will _never_ be `nil`, but _will_ represent a nonexistent/invalid game entity if the `Loaded` property is `false`.",
		"This property is shorthand for `Entity.Mount`.")]
	public MountData Mount => this.Entity.Mount;

	#region Player display

	[LuaPlayerDoc("This is the text value of the current character's name.",
		"Per FFXIV name formatting, it will contain the first name, a single space, and the last name.",
		"See also the `Firstname` and `Lastname` properties.")]
	public string? Name => this.Loaded
		? Service.ClientState.LocalPlayer!.Name!.TextValue
		: null;

	[LuaPlayerDoc("This is the first name (and only the first name) of the current character.",
		"Given FFXIV's name formatting, you can concatenate this property, a single space, and the `Lastname` property to produce the character's full name.")]
	public string? Firstname => this.Loaded
		? this.Name!.Split(' ')[0]
		: null;

	[LuaPlayerDoc("This is the last name (and only the last name) of the current character.",
		"Given FFXIV's name formatting, you can concatenate the `Firstname` property, a single space, and this property to produce the character's full name.")]
	public string? Lastname => this.Loaded
		? this.Name!.Split(' ')[1]
		: null;

	[LuaPlayerDoc("This indicates whether or not the current character is using a title.",
		"If this is `false`, the `TitleText` property will be an empty string, and `TitleIsPrefix` will be `nil`.",
		"This property is shorthand for `Entity.HasTitle`.")]
	public bool? HasTitle => this.Entity.HasTitle;
	[LuaPlayerDoc("This is the plain text value of the current character's current title, respecting gender-adaptive titles.",
		"For example, a male character will have `Master of the Land` while a female character will have `Mistress of the Land` instead.",
		"If the current character doesn't have a title (`HasTitle == false`) this will be an empty string.",
		"This property is shorthand for `Entity.TitleText`.")]
	public string? TitleText => this.Entity.TitleText;
	[LuaPlayerDoc("This indicates whether or not the current character's current title (if any) is a \"prefix\" title or a \"postfix\" title.",
		"Prefix titles are displayed above the name in the nameplate, while postfix titles are shown after.",
		"Note that if the current character doesn't have a title (`HasTitle == false`) this will be `nil`.",
		"This property is shorthand for `Entity.TitleIsPrefix`.")]
	public bool? TitleIsPrefix => this.Entity.TitleIsPrefix;

	[LuaPlayerDoc("This is the \"tag\" (short abbreviation shown in the nameplate) for the current character's Free Company.",
		"If the current character is not in a Free Company, this will be an empty string.",
		"This property is shorthand for `Entity.CompanyTag`.")]
	public string? CompanyTag => this.Entity.CompanyTag;

	#endregion

	#region Gender

	[LuaPlayerDoc("This indicates whether the game considers the current character to be male.",
		"All entities in the game are either male or female, including things that aren't alive. The game does not support a third state, even just one of \"entity has no gender\".",
		"This property is shorthand for `Entity.IsMale`.")]
	public bool? IsMale => this.Entity.IsMale;
	[LuaPlayerDoc("The inverse of `IsMale`, this indicates whether the game considers the current character to be female.",
		"All entities in the game are either male or female, including things that aren't alive. The game does not support a third state, even just one of \"entity has no gender\".",
		"This property is shorthand for `Entity.IsFemale`.")]
	public bool? IsFemale => this.Entity.IsFemale;
	[LuaPlayerDoc("This indicates whether the entity is considered either male _or_ female. This is expected to never be `false`, but is included in case of future expansion.",
		"This property is shorthand for `Entity.IsGendered`.")]
	public bool? IsGendered => this.Entity.IsGendered;

	[SkipDoc("It's an internally-useful string-only version of the below")]
	public string? MF(string male, string female) => this.Entity.MF(male, female);
	[SkipDoc("It's an internally-useful string-only version of the below")]
	public string? MFN(string male, string female, string neither) => this.Entity.MFN(male, female, neither);

	[LuaPlayerDoc("This function is intended to simplify gender-adaptive code by returning the first value if the current character is male, or the second if they are female.",
		"In the event that the current character is not gendered by the game (which should never happen, barring a significant engine rewrite), this will return `nil`.")]
	[return: AsLuaType(LuaType.Any)]
	public DynValue MF([AsLuaType(LuaType.Any)] DynValue male, [AsLuaType(LuaType.Any)] DynValue female) => this.Entity.MF(male, female);
	[LuaPlayerDoc("This function is intended to simplify gender-adaptive code by returning the first value if the current character is male, the second if they are female, or the third if they are not gendered.",
		"Note that, barring a significant engine rewrite, the game should never consider any entity to be ungendered. This feature is included in case of future expansion.")]
	[return: AsLuaType(LuaType.Any)]
	public DynValue MFN([AsLuaType(LuaType.Any)] DynValue male, DynValue female, [AsLuaType(LuaType.Any)] DynValue neither) => this.Entity.MFN(male, female, neither);

	#endregion

	#region Worlds

	[LuaPlayerDoc("This is the _internal numeric ID_ of the player's HOME world.",
		"For most purposes, you'll probably want `HomeWorld` instead for the name of it.")]
	public ushort? HomeWorldId => this.Entity.HomeWorldId;
	[LuaPlayerDoc("This is the _plain text name_ of the player's HOME world.",
		"If you're trying to check if they're on their home world, you may wish to use `HomeWorldId` and `CurrentWorldId` instead.")]
	public string? HomeWorld => this.Entity.HomeWorld;

	[LuaPlayerDoc("This is the _internal numeric ID_ of the player's CURRENT world.",
		"For most purposes, you'll probably want `CurrentWorld` instead for the name of it.")]
	public ushort? CurrentWorldId => this.Entity.CurrentWorldId;
	[LuaPlayerDoc("This is the _plain text name_ of the player's CURRENT world.",
		"If you're trying to check if they're on their home world, you may wish to use `HomeWorldId` and `CurrentWorldId` instead.")]
	public string? CurrentWorld => this.Entity.CurrentWorld;

	#endregion

	#region Stats

	public byte? Level => this.Entity.Level;

	public JobData Job => this.Entity.Job;

	public uint? Hp => this.Entity.Hp;
	public uint? MaxHp => this.Entity.MaxHp;

	public uint? Mp => this.Entity.Mp;
	public uint? MaxMp => this.Entity.MaxMp;

	public uint? Cp => this.Entity.Cp;
	public uint? MaxCp => this.Entity.MaxCp;

	public uint? Gp => this.Entity.Gp;
	public uint? MaxGp => this.Entity.MaxGp;

	#endregion

	#region Location

	public uint? MapZone
		=> this.Loaded
			? Service.ClientState.TerritoryType
		: null;

	// X and Z are the horizontal coordinates, Y is the vertical one
	public float? PosX => this.Entity.PosX;
	public float? PosY => this.Entity.PosY;
	public float? PosZ => this.Entity.PosZ;

	public double? RotationRadians => this.Entity.RotationRadians;
	public double? RotationDegrees => this.Entity.RotationDegrees;

	#endregion

	#region Condition flags

	public bool? InCombat
		=> this.Loaded
			? Service.Condition[ConditionFlag.InCombat]
			|| Service.ClientState.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.InCombat)
		: null;

	public bool Mounted => this.Mount.Active;

	public bool? Crafting
		=> this.Loaded
			? Service.Condition[ConditionFlag.Crafting]
			|| Service.Condition[ConditionFlag.Crafting40]
			|| Service.Condition[ConditionFlag.PreparingToCraft]
		: null;

	public bool? Gathering
		=> this.Loaded
			? Service.Condition[ConditionFlag.Gathering]
			|| Service.Condition[ConditionFlag.Gathering42]
		: null;

	public bool? Fishing
		=> this.Loaded
			? Service.Condition[ConditionFlag.Fishing]
		: null;

	public bool? Performing
		=> this.Loaded
			? Service.Condition[ConditionFlag.Performing]
		: null;

	public bool? Casting
		=> this.Loaded
			? Service.Condition[ConditionFlag.Casting]
			|| Service.Condition[ConditionFlag.Casting87]
			|| Service.ClientState.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.IsCasting)
		: null;

	public bool? InCutscene
		=> this.Loaded
			? Service.Condition[ConditionFlag.WatchingCutscene]
			|| Service.Condition[ConditionFlag.WatchingCutscene78]
			|| Service.Condition[ConditionFlag.OccupiedInCutSceneEvent]
			|| Service.Condition[ConditionFlag.BetweenAreas]
			|| Service.Condition[ConditionFlag.BetweenAreas51]
		: null;

	public bool? Trading
		=> this.Loaded
			? Service.Condition[ConditionFlag.TradeOpen]
		: null;

	public bool? Flying
		=> this.Loaded
			? Service.Condition[ConditionFlag.InFlight]
		: null;

	public bool? Swimming
		=> this.Loaded
			? Service.Condition[ConditionFlag.Swimming]
		: null;

	public bool? Diving
		=> this.Loaded
			? Service.Condition[ConditionFlag.Diving]
		: null;

	public bool? Jumping
		=> this.Loaded
			? Service.Condition[ConditionFlag.Jumping]
			|| Service.Condition[ConditionFlag.Jumping61]
		: null;

	public bool? InDuty
		=> this.Loaded
			? Service.Condition[ConditionFlag.BoundByDuty]
			|| Service.Condition[ConditionFlag.BoundByDuty56]
			|| Service.Condition[ConditionFlag.BoundByDuty95]
			|| Service.Condition[ConditionFlag.BoundToDuty97]
		: null;

	public bool? UsingFashionAccessory
		=> this.Loaded
			? Service.Condition[ConditionFlag.UsingParasol]
		: null;

	public bool? WeaponDrawn
		=> this.Loaded
			? Service.ClientState.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.WeaponOut)
		: null;

	public static unsafe bool Moving
		=> AgentMap.Instance() is not null && AgentMap.Instance()->IsPlayerMoving > 0;

	#endregion

	#region Party/alliance

	public PartyApi Party { get; private set; } = null!;

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

	internal static void initialiseEmotes() {
		if (emotesLoaded)
			return;
		emotesLoaded = true;
		using MethodTimer logtimer = new();
		Service.Log.Information($"[{LogTag.Emotes}] Initialising API data");

		ExcelSheet<Emote> emotes = Service.DataManager.GameData.GetExcelSheet<Emote>()!;
		try {
			uint max = emotes.RowCount;
			Service.Log.Information($"[{LogTag.Emotes}] Indexing {max:N0} emotes...");
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
			Service.Log.Information($"[{LogTag.Emotes}] Cached {emoteUnlocks.Count:N0} emote names");
		}
		catch (Exception e) {
			Service.Plugin.Error("Unable to load Emote sheet, cannot check emote unlock state!", e);
		}

	}

	public unsafe bool? HasEmote(string emote) {
		if (!this.Loaded)
			return null;

		string internalName = emote.TrimStart('/');
		this.Log($"Checking whether '{internalName}' is unlocked", LogTag.Emotes);
		if (!emoteUnlocks.TryGetValue(internalName, out uint unlockLink)) {
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
