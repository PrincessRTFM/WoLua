using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
public class PlayerApi: ApiBase {

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
		"It is recommended that you only cache this in a function-local variable, and not rely on it remaining valid between execution frames, especially if you use the action queue.",
		"This value itself will _never_ be `nil`, but _will_ represent a nonexistent/invalid game entity if the `Loaded` property is `false`.")]
	public EntityWrapper Entity => new(this ? Service.ClientState.LocalPlayer : null);

	[LuaDoc("This provides a `MountData` wrapper object around the currently-logged-in player's mount (or around nothing) _at the time of access_.",
		"If you cache this, it may become invalid, such as if the player logs out, changes mounts, or dismounts entirely.",
		"It is recommended that you only cache this in a function-local variable, and not rely on it remaining valid between execution frames, especially if you use the action queue.",
		"This value itself will _never_ be `nil`, but _will_ represent a nonexistent/invalid game entity if the `Loaded` property is `false`.",
		"This property is shorthand for `.Entity.Mount`.")]
	public MountWrapper Mount => this.Entity.Mount;

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
		"This property is shorthand for `.Entity.HasTitle`.")]
	public bool? HasTitle => this.Entity.HasTitle;
	[LuaPlayerDoc("This is the plain text value of the current character's current title, respecting gender-adaptive titles.",
		"For example, a male character will have `Master of the Land` while a female character will have `Mistress of the Land` instead.",
		"If the current character doesn't have a title (`HasTitle == false`) this will be an empty string.",
		"This property is shorthand for `.Entity.TitleText`.")]
	public string? TitleText => this.Entity.TitleText;
	[LuaPlayerDoc("This indicates whether or not the current character's current title (if any) is a \"prefix\" title or a \"postfix\" title.",
		"Prefix titles are displayed above the name in the nameplate, while postfix titles are shown after.",
		"Note that if the current character doesn't have a title (`HasTitle == false`) this will be `nil`.",
		"This property is shorthand for `.Entity.TitleIsPrefix`.")]
	public bool? TitleIsPrefix => this.Entity.TitleIsPrefix;

	[LuaPlayerDoc("This is the \"tag\" (short abbreviation shown in the nameplate) for the current character's Free Company.",
		"If the current character is not in a Free Company, this will be an empty string.",
		"This property is shorthand for `.Entity.CompanyTag`.")]
	public string? CompanyTag => this.Entity.CompanyTag;

	#endregion

	#region Gender

	[LuaPlayerDoc("This indicates whether the game considers the current character to be male.",
		"All entities in the game are either male or female, including things that aren't alive. The game does not support a third state, even just one of \"entity has no gender\".",
		"This property is shorthand for `.Entity.IsMale`.")]
	public bool? IsMale => this.Entity.IsMale;
	[LuaPlayerDoc("The inverse of `IsMale`, this indicates whether the game considers the current character to be female.",
		"All entities in the game are either male or female, including things that aren't alive. The game does not support a third state, even just one of \"entity has no gender\".",
		"This property is shorthand for `.Entity.IsFemale`.")]
	public bool? IsFemale => this.Entity.IsFemale;
	[LuaPlayerDoc("This indicates whether the entity is considered either male _or_ female. This is expected to never be `false`, but is included in case of future expansion.",
		"This property is shorthand for `.Entity.IsGendered`.")]
	public bool? IsGendered => this.Entity.IsGendered;

	[SkipDoc("It's an internally-useful string-only version of the below")]
	public string? MF(string male, string female) => this.Entity.MF(male, female);
	[SkipDoc("It's an internally-useful string-only version of the below")]
	public string? MFN(string male, string female, string neither) => this.Entity.MFN(male, female, neither);

	[LuaPlayerDoc("This function is intended to simplify gender-adaptive code by returning the first value if the current character is male, or the second if they are female.",
		"In the event that the current character is not gendered by the game (which should never happen, barring a significant engine rewrite), this will return `nil`.",
		"This method is shorthand for `.Entity.MF(male, female)`.")]
	[return: AsLuaType(LuaType.Any)]
	public DynValue MF([AsLuaType(LuaType.Any)] DynValue male, [AsLuaType(LuaType.Any)] DynValue female) => this.Entity.MF(male, female);
	[LuaPlayerDoc("This function is intended to simplify gender-adaptive code by returning the first value if the current character is male, the second if they are female, or the third if they are not gendered.",
		"Note that, barring a significant engine rewrite, the game should never consider any entity to be ungendered. This feature is included in case of future expansion.",
		"This method is shorthand for `.Entity.MFN(male, female, neither)`.")]
	[return: AsLuaType(LuaType.Any)]
	public DynValue MFN([AsLuaType(LuaType.Any)] DynValue male, DynValue female, [AsLuaType(LuaType.Any)] DynValue neither) => this.Entity.MFN(male, female, neither);

	#endregion

	#region Worlds

	[LuaPlayerDoc("This is the _internal numeric ID_ of the player's HOME world.",
		"For most purposes, you'll probably want `HomeWorld` instead for the name of it.",
		"This property is shorthand for `.Entity.HomeWorldId`.")]
	public ushort? HomeWorldId => this.Entity.HomeWorldId;
	[LuaPlayerDoc("This is the _plain text name_ of the player's HOME world.",
		"If you're trying to check if they're on their home world, you may wish to use `HomeWorldId` and `CurrentWorldId` instead.",
		"This property is shorthand for `.Entity.HomeWorld`.")]
	public string? HomeWorld => this.Entity.HomeWorld;

	[LuaPlayerDoc("This is the _internal numeric ID_ of the player's CURRENT world.",
		"For most purposes, you'll probably want `CurrentWorld` instead for the name of it.",
		"This property is shorthand for `.Entity.CurrentWorldId`.")]
	public ushort? CurrentWorldId => this.Entity.CurrentWorldId;
	[LuaPlayerDoc("This is the _plain text name_ of the player's CURRENT world.",
		"If you're trying to check if they're on their home world, you may wish to use `HomeWorldId` and `CurrentWorldId` instead.",
		"This property is shorthand for `.Entity.CurrentWorld`.")]
	public string? CurrentWorld => this.Entity.CurrentWorld;

	#endregion

	#region Stats

	[LuaPlayerDoc("The current character's level, including level sync effects.",
		"This property is shorthand for `.Entity.Level`.")]
	public byte? Level => this.Entity.Level;

	[LuaDoc("The current character's job data.",
		"This will never be nil, but the returned object may represent an invalid job if the player isn't loaded.",
		"This property is shorthand for `.Entity.Job`.")]
	public JobData Job => this.Entity.Job;

	[LuaPlayerDoc("The current character's _current_ health.",
		"This property is shorthand for `.Entity.Hp`.")]
	public uint? Hp => this.Entity.Hp;
	[LuaPlayerDoc("The current character's _maximum_ health.",
		"This property is shorthand for `.Entity.MaxHp`.")]
	public uint? MaxHp => this.Entity.MaxHp;

	[LuaPlayerDoc("The current character's _current_ mana.",
		"This property is shorthand for `.Entity.Mp`.")]
	public uint? Mp => this.Entity.Mp;
	[LuaPlayerDoc("The current character's _maximum_ mana.",
		"At present, all combat classes and jobs always have 10,000 maximum mana.",
		"If the player is not on a combat class or job, the value is indeterminate and may be meaningless.",
		"This property is shorthand for `.Entity.MaxMp`.")]
	public uint? MaxMp => this.Entity.MaxMp;

	[LuaPlayerDoc("The current character's _current_ crafting points.",
		"Only guaranteed to be valid if the player is on a DoH class. Otherwise, the value is indeterminate and may be meaningless.",
		"This property is shorthand for `.Entity.Cp`.")]
	public uint? Cp => this.Entity.Cp;
	[LuaPlayerDoc("The current character's _maximum_ crafting points.",
		"Only guaranteed to be valid if the player is on a DoH class. Otherwise, the value is indeterminate and may be meaningless.",
		"This property is shorthand for `.Entity.MaxCp`.")]
	public uint? MaxCp => this.Entity.MaxCp;

	[LuaPlayerDoc("The current character's _current_ gathering points.",
		"Only guaranteed to be valid if the player is on a DoL class. Otherwise, the value is indeterminate and may be meaningless.",
		"This property is shorthand for `.Entity.Gp`.")]
	public uint? Gp => this.Entity.Gp;
	[LuaPlayerDoc("The current character's _maximum_ gathering points.",
		"Only guaranteed to be valid if the player is on a DoL class. Otherwise, the value is indeterminate and may be meaningless.",
		"This property is shorthand for `.Entity.MaxGp`.")]
	public uint? MaxGp => this.Entity.MaxGp;

	#endregion

	#region Location

	[LuaPlayerDoc("The raw (internal) numeric (unsigned integer) ID of the current character's current map zone.",
		"This is always the same number for the same zone, regardless of client language, so it can safely be used to check where the player currently is.",
		"This may be 0 if the player somehow hasn't loaded into a zone.")]
	public uint? MapZone => this.Loaded
		? Service.ClientState.TerritoryType
		: null;

	// X and Z are the horizontal coordinates, Y is the vertical one
	// But that's not how the game displays things to the player, because fuck you I guess, so we swap those two around for consistency
	// That's done in EntityWrapper since these methods call to it, but the docs here have to match, so if you're confused, now you know

	[LuaPlayerDoc("The raw (internal) X coordinate of the current character.",
		"This represents the east/west position.",
		"This property is shorthand for `.Entity.PosX`.")]
	public float? PosX => this.Entity.PosX;

	[LuaPlayerDoc("The raw (internal) Y coordinate of the current character.",
		"This represents the north/south position.",
		"Please note that this is _actually_ the internal _Z_ coordinate, since the game engine uses X/Z for horizontal position and Y for vertical.",
		"For the sake of consistency with the map coordinates displayed to the player, " + Plugin.Name + " swaps them.",
		"This property is shorthand for `.Entity.PosY`.")]
	public float? PosY => this.Entity.PosY;

	[LuaPlayerDoc("The raw (internal) Z coordinate of the current character.",
		"This represents the vertical position.",
		"Please note that this is _actually_ the internal _Y_ coordinate, since the game engine uses X/Z for horizontal position and Y for vertical.",
		"For the sake of consistency with the map coordinates displayed to the player, " + Plugin.Name + " swaps them.",
		"This property is shorthand for `.Entity.PosZ`.")]
	public float? PosZ => this.Entity.PosZ;

	[LuaPlayerDoc("The player-friendly map-style X (east/west) coordinate of the current character.",
		"This property is shorthand for `.Entity.MapX`.")]
	public float? MapX => this.Entity.MapX;

	[LuaPlayerDoc("The player-friendly map-style Y (north/south) coordinate of the current character.",
		"This property is shorthand for `.Entity.MapY`.")]
	public float? MapY => this.Entity.MapY;

	[LuaPlayerDoc("The player-friendly map-style Z (height) coordinate of the current character.",
		"This property is shorthand for `.Entity.MapZ`.")]
	public float? MapZ => this.Entity.MapZ;

	[LuaDoc("Provides three values consisting of the current character's X (east/west), Y (north/south), and Z (vertical) coordinates.",
		"If the player isn't loaded, all three values will be nil.",
		"This property is shorthand for `.Entity.MapCoords`.")]
	public DynValue MapCoords => this.Entity.MapCoords;

	[LuaPlayerDoc("The current character's rotation in radians, ranging from 0 to 2pi.",
		"This property is shorthand for `.Entity.RotationRadians`.")]
	public double? RotationRadians => this.Entity.RotationRadians;

	[LuaPlayerDoc("The current character's rotation in degrees, ranging from 0 to 360.",
		"This property is shorthand for `.Entity.RotationDegrees`.")]
	public double? RotationDegrees => this.Entity.RotationDegrees;

	#endregion

	#region Condition flags

	[LuaPlayerDoc("Whether the current character is engaged in combat, which restricts certain actions.")]
	public bool? InCombat => this.Loaded
		? Service.Condition[ConditionFlag.InCombat]
		|| Service.ClientState.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.InCombat)
		: null;

	[LuaPlayerDoc("Whether the currect character is presently mounted. It may not be their own mount.")]
	public bool Mounted => this.Mount.Active;

	[LuaPlayerDoc("Whether the current character is performing crafting.")]
	public bool? Crafting => this.Loaded
		? Service.Condition[ConditionFlag.Crafting]
		|| Service.Condition[ConditionFlag.Crafting40]
		|| Service.Condition[ConditionFlag.PreparingToCraft]
		: null;

	[LuaPlayerDoc("Whether the current character is engaged with a gathering node.")]
	public bool? Gathering => this.Loaded
		? Service.Condition[ConditionFlag.Gathering]
		|| Service.Condition[ConditionFlag.Gathering42]
		: null;

	[LuaPlayerDoc("Whether the current character is actively fishing.")]
	public bool? Fishing => this.Loaded
		? Service.Condition[ConditionFlag.Fishing]
		: null;

	[LuaPlayerDoc("Whether the current character is performing music. Only meaningful for bards.")]
	public bool? Performing => this.Loaded
		? Service.Condition[ConditionFlag.Performing]
		: null;

	[LuaPlayerDoc("Whether the current character is \"casting\". It is not currently known if this includes other actions with a progress bar.")]
	public bool? Casting => this.Loaded
		? Service.Condition[ConditionFlag.Casting]
		|| Service.Condition[ConditionFlag.Casting87]
		|| Service.ClientState.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.IsCasting)
		: null;

	[LuaPlayerDoc("Whether the current character is watching a cutscene. Some commands are not available during cutscenes.")]
	public bool? InCutscene => this.Loaded
		? Service.Condition[ConditionFlag.WatchingCutscene]
		|| Service.Condition[ConditionFlag.WatchingCutscene78]
		|| Service.Condition[ConditionFlag.OccupiedInCutSceneEvent]
		|| Service.Condition[ConditionFlag.BetweenAreas]
		|| Service.Condition[ConditionFlag.BetweenAreas51]
		: null;

	[LuaPlayerDoc("Whether the current character has a trade window open.")]
	public bool? Trading => this.Loaded
		? Service.Condition[ConditionFlag.TradeOpen]
		: null;

	[LuaPlayerDoc("Whether the current character is considered to be flying.")]
	public bool? Flying => this.Loaded
		? Service.Condition[ConditionFlag.InFlight]
		: null;

	[LuaPlayerDoc("Whether the current character is swimming in water. This is not the same as diving UNDER water.")]
	public bool? Swimming => this.Loaded
		? Service.Condition[ConditionFlag.Swimming]
		: null;

	[LuaPlayerDoc("Whether the current character is underwater.")]
	public bool? Diving => this.Loaded
		? Service.Condition[ConditionFlag.Diving]
		: null;

	[LuaPlayerDoc("Whether the current character is in a jump.")]
	public bool? Jumping => this.Loaded
		? Service.Condition[ConditionFlag.Jumping]
		|| Service.Condition[ConditionFlag.Jumping61]
		: null;

	[LuaPlayerDoc("Whether the current character is within an instanced duty.")]
	public bool? InDuty => this.Loaded
		? Service.Condition[ConditionFlag.BoundByDuty]
		|| Service.Condition[ConditionFlag.BoundByDuty56]
		|| Service.Condition[ConditionFlag.BoundByDuty95]
		|| Service.Condition[ConditionFlag.BoundToDuty97]
		: null;

	[LuaPlayerDoc("Whether the current character is using a fashion accessory, such as a parasol or set of wings.")]
	public bool? UsingFashionAccessory => this.Loaded
		? Service.Condition[ConditionFlag.UsingParasol]
		: null;

	[LuaPlayerDoc("Whether the current character has their weapon drawn.")]
	public bool? WeaponDrawn => this.Loaded
		? Service.ClientState.LocalPlayer!.StatusFlags.HasFlag(StatusFlags.WeaponOut)
		: null;

	[LuaPlayerDoc("Whether the game considers the current character to be in motion.")]
	public unsafe bool Moving => AgentMap.Instance() is not null && AgentMap.Instance()->IsPlayerMoving > 0;

	#endregion

	#region Party/alliance

	public PartyApi Party { get; private set; } = null!;

	#endregion

	#region Targets

	[LuaDoc("Returns a new EntityWrapper for the current character's hard target.",
		"This will never be `nil` - if there is no hard target, the returned wrapper will represent nothing.",
		"You may set this to `nil` to clear the current hard target.")]
	[NotNull]
	public EntityWrapper? Target {
		get => new(this.Loaded ? Service.Targets.Target : null);
		set {
			if (this.Loaded)
				Service.Targets.Target = value?.Entity;
		}
	}
	[LuaDoc("Whether the current character has a hard target.",
		"This is identical to checking whether `.Target.Exists` is true.")]
	public bool? HasTarget => this.Loaded ? this.Target : null;
	[LuaDoc("Clears the current character's hard target.",
		"This is identical to setting `.Target = nil`, but easier to use with script action queues.")]
	public void ClearTarget() => Service.Targets.Target = null;

	[LuaDoc("Returns a new EntityWrapper for the current character's soft target.",
		"This will never be `nil` - if there is no soft target, the returned wrapper will represent nothing.",
		"You may set this to `nil` to clear the current soft target.")]
	[NotNull]
	public EntityWrapper? SoftTarget {
		get => new(this.Loaded ? Service.Targets.SoftTarget : null);
		set {
			if (this.Loaded)
				Service.Targets.SoftTarget = value?.Entity;
		}
	}
	[LuaDoc("Whether the current character has a soft target.",
		"This is identical to checking whether `.SoftTarget.Exists` is true.")]
	public bool? HasSoftTarget => this.Loaded ? this.SoftTarget : null;
	[LuaDoc("Clears the current character's soft target.",
		"This is identical to setting `.SoftTarget = nil`, but easier to use with script action queues.")]
	public void ClearSoftTarget() => Service.Targets.SoftTarget = null;

	[LuaDoc("Returns a new EntityWrapper for the current character's focus target.",
		"This will never be `nil` - if there is no focus target, the returned wrapper will represent nothing.",
		"You may set this to `nil` to clear the current focus target.")]
	[NotNull]
	public EntityWrapper? FocusTarget {
		get => new(this.Loaded ? Service.Targets.FocusTarget : null);
		set {
			if (this.Loaded)
				Service.Targets.FocusTarget = value?.Entity;
		}
	}
	[LuaDoc("Whether the current character has a focus target.",
		"This is identical to checking whether `.FocusTarget.Exists` is true.")]
	public bool? HasFocusTarget => this.Loaded ? this.FocusTarget : null;
	[LuaDoc("Clears the current character's focus target.",
		"This is identical to setting `.FocusTarget = nil`, but easier to use with script action queues.")]
	public void ClearFocusTarget() => Service.Targets.FocusTarget = null;

	[LuaDoc("Returns a new EntityWrapper for the current character's FIELD mouseover target.",
		"This will never be `nil` - if there is no such target, the returned wrapper will represent nothing.")]
	public EntityWrapper FieldMouseOverTarget => new(this.Loaded ? Service.Targets.MouseOverTarget : null);
	[LuaDoc("Whether the current character has a FIELD mouseover target.",
		"This is identical to checking whether `.FieldMouseOverTarget.Exists` is true.")]
	public bool? HasFieldMouseOverTarget => this.Loaded ? this.MouseOverTarget : null;

	[LuaDoc("Returns a new EntityWrapper for the current character's UI mouseover target.",
		"This will never be `nil` - if there is no such target, the returned wrapper will represent nothing.")]
	public EntityWrapper UiMouseOverTarget => new(this.Loaded ? Service.Hooks.UITarget : null);
	[LuaDoc("Whether the current character has a UI mouseover target.",
		"This is identical to checking whether `.UiMouseOverTarget.Exists` is true.")]
	public bool? HasUiMouseOverTarget => this.Loaded ? this.MouseOverTarget : null;

	[LuaDoc("Returns a new EntityWrapper for the current character's mouseover target, be it UI or field.",
		"If there is a UI mouseover target, it will be returned. Otherwise, the field target will be returned if it exists. Finally, an empty entity wrapper will be used.")]
	public EntityWrapper MouseOverTarget {
		get {
			if (!this.Loaded)
				return EntityWrapper.Empty;
			EntityWrapper found = this.UiMouseOverTarget;
			if (!found)
				found = this.FieldMouseOverTarget;
			return found;
		}
	}
	[LuaDoc("Whether the current character has a mouseover target, be it UI or field.",
		"This is identical to checking whether `.MouseOverTarget.Exists` is true.")]
	public bool? HasMouseOverTarget => this.Loaded ? this.MouseOverTarget : null;

	#endregion

	#region Emotes

	private static bool emotesLoaded = false;
	private static readonly Dictionary<string, uint> emoteUnlocks = new();

	internal static void InitialiseEmotes() {
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
						.Select(s => s.TrimStart('/'))
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

	[LuaPlayerDoc("Determines whether the current character has unlocked a given emote.",
		"The emote name should be one of the emote's commands, with or without the leading `/` character.")]
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

	[LuaDoc("Get the `Firstname Lastname@Homeworld` formatted name for the current character, suitable for use in things like `/tell` commands.",
		"If the user is not logged in, this will be an empty string, NOT `nil`.")]
	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString()
		=> this.Loaded
			? $"{this.Name}@{this.Entity.HomeWorld}"
		: string.Empty;

	#endregion
}
