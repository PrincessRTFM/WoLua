namespace PrincessRTFM.WoLua.Lua.Api.Game;

using System;
using System.Numerics;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility.Numerics;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

using CSChar = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

[MoonSharpUserData]
public sealed record class EntityWrapper(GameObject? Entity): IEquatable<EntityWrapper> {
	private unsafe CSChar* cs => this.IsPlayer ? (CSChar*)this.Entity!.Address : null;

	public static implicit operator GameObject?(EntityWrapper? wrapper) => wrapper?.Entity;
	public static implicit operator EntityWrapper(GameObject? entity) => new(entity);

	public bool Exists => this.Entity is not null && this.Entity.IsValid() && this.Entity.ObjectKind is not ObjectKind.None;
	public static implicit operator bool(EntityWrapper? entity) => entity?.Exists ?? false;

	public string? Name => this
		? this.Entity?.Name?.TextValue ?? string.Empty
		: null;

	public string? Type => this ? this.Entity!.ObjectKind.ToString() : null;

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => this ? $"{this.Type}[{this.Entity!.Name ?? string.Empty}]" : string.Empty;

	public bool? Alive => this ? !this.Entity?.IsDead : null;

	public unsafe MountData Mount {
		get {
			CSChar* player = this.cs;
			if (player is null)
				return new(0);
			CSChar.MountContainer? mount = player->IsMounted() ? player->Mount : null;
			return new(mount?.MountId ?? 0);
		}
	}

	#region Worlds

	public ushort? HomeWorldId => this.IsPlayer && this.Entity is PlayerCharacter p ? (ushort)p.HomeWorld.GameData!.RowId : null;
	public string? HomeWorld => this.IsPlayer && this.Entity is PlayerCharacter p ? p.HomeWorld.GameData!.Name!.RawString : null;

	public ushort? CurrentWorldId => this.IsPlayer && this.Entity is PlayerCharacter p ? (ushort)p.CurrentWorld.GameData!.RowId : null;
	public string? CurrentWorld => this.IsPlayer && this.Entity is PlayerCharacter p ? p.CurrentWorld.GameData!.Name!.RawString : null;

	#endregion

	#region Entity type

	public bool IsPlayer => this && this.Entity?.ObjectKind is ObjectKind.Player;
	public bool IsCombatNpc => this && this.Entity?.ObjectKind is ObjectKind.BattleNpc;
	public bool IsTalkNpc => this && this.Entity?.ObjectKind is ObjectKind.EventNpc;
	public bool IsNpc => this.IsCombatNpc || this.IsTalkNpc;
	public bool IsTreasure => this && this.Entity?.ObjectKind is ObjectKind.Treasure;
	public bool IsAetheryte => this && this.Entity?.ObjectKind is ObjectKind.Aetheryte;
	public bool IsGatheringNode => this && this.Entity?.ObjectKind is ObjectKind.GatheringPoint;
	public bool IsEventObject => this && this.Entity?.ObjectKind is ObjectKind.EventObj;
	public bool IsMount => this && this.Entity?.ObjectKind is ObjectKind.MountType;
	public bool IsMinion => this && this.Entity?.ObjectKind is ObjectKind.Companion;
	public bool IsRetainer => this && this.Entity?.ObjectKind is ObjectKind.Retainer;
	public bool IsArea => this && this.Entity?.ObjectKind is ObjectKind.Area;
	public bool IsHousingObject => this && this.Entity?.ObjectKind is ObjectKind.Housing;
	public bool IsCutsceneObject => this && this.Entity?.ObjectKind is ObjectKind.Cutscene;
	public bool IsCardStand => this && this.Entity?.ObjectKind is ObjectKind.CardStand;
	public bool IsOrnament => this && this.Entity?.ObjectKind is ObjectKind.Ornament;

	#endregion

	#region Stats

	public byte? Level => this && this.Entity is Character self ? self.Level : null;

	public JobData Job {
		get {
			if (this && this.Entity is Character self)
				return new(self.ClassJob!.Id, self.ClassJob!.GameData!.Name!.ToString().ToLower(), self.ClassJob!.GameData!.Abbreviation!.ToString().ToUpper());
			return new(0, JobData.InvalidJobName, JobData.InvalidJobAbbr);
		}
	}

	public string? CompanyTag => this && this.Entity is Character self ? self.CompanyTag.TextValue : null;

	public uint? Hp => this && this.Entity is Character self && self.MaxHp > 0 ? self.CurrentHp : null;
	public uint? MaxHp => this && this.Entity is Character self ? self.MaxHp : null;

	public uint? Mp => this && this.Entity is Character self && self.MaxMp > 0 ? self.CurrentMp : null;
	public uint? MaxMp => this && this.Entity is Character self ? self.MaxMp : null;

	public uint? Cp => this && this.Entity is Character self && self.MaxCp > 0 ? self.CurrentCp : null;
	public uint? MaxCp => this && this.Entity is Character self ? self.MaxCp : null;

	public uint? Gp => this && this.Entity is Character self && self.MaxGp > 0 ? self.CurrentGp : null;
	public uint? MaxGp => this && this.Entity is Character self ? self.MaxGp : null;

	#endregion

	#region Flags

	public bool IsHostile => this && this.Entity is Character self && self.StatusFlags.HasFlag(StatusFlags.Hostile);
	public bool InCombat => this && this.Entity is Character self && self.StatusFlags.HasFlag(StatusFlags.InCombat);
	public bool WeaponDrawn => this && this.Entity is Character self && self.StatusFlags.HasFlag(StatusFlags.WeaponOut);
	public bool IsPartyMember => this && this.Entity is Character self && self.StatusFlags.HasFlag(StatusFlags.PartyMember);
	public bool IsAllianceMember => this && this.Entity is Character self && self.StatusFlags.HasFlag(StatusFlags.AllianceMember);
	public bool IsFriend => this && this.Entity is Character self && self.StatusFlags.HasFlag(StatusFlags.Friend);
	public bool IsCasting => this && this.Entity is BattleChara self && self.IsCasting;
	public bool CanInterrupt => this && this.Entity is BattleChara self && self.IsCasting && self.IsCastInterruptible;

	#endregion

	#region Position
	// X and Z are the horizontal coordinates, Y is the vertical one

	public float? PosX => this ? this.Entity!.Position.X : null;
	public float? PosY => this ? this.Entity!.Position.Y : null;
	public float? PosZ => this ? this.Entity!.Position.Z : null;

	public double? RotationRadians => this ? this.Entity!.Rotation + Math.PI : null;
	public double? RotationDegrees => this ? this.RotationRadians!.Value * 180 / Math.PI : null;

	#endregion

	#region Distance

	public float? FlatDistanceFrom(EntityWrapper? other) => this.Exists && (other?.Exists ?? false)
		? Vector3.Distance(this.Entity!.Position.WithY(0), other!.Entity!.Position.WithY(0))
		: null;
	public float? FlatDistanceFrom(PlayerApi player) => this.FlatDistanceFrom(player.Entity);

	public float? DistanceFrom(EntityWrapper? other) => this.Exists && (other?.Exists ?? false)
		? Vector3.Distance(this.Entity!.Position, other.Entity!.Position)
		: null;
	public float? DistanceFrom(PlayerApi player) => this.DistanceFrom(player.Entity);

	public float? FlatDistance => this.FlatDistanceFrom(Service.Client.LocalPlayer);
	public float? Distance => this.DistanceFrom(Service.Client.LocalPlayer);

	#endregion

	#region IEquatable
	public bool Equals(EntityWrapper? other) => this.Entity == other?.Entity;
	public override int GetHashCode() => this.Entity?.GetHashCode() ?? 0;
	#endregion

}
