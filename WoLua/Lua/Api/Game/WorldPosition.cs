using System;
using System.Numerics;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;

using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Deconstruct))]
public sealed record class WorldPosition(float? PosX, float? PosY, float? PosZ): IWorldObjectWrapper, IEquatable<WorldPosition>, IComparable<WorldPosition> {
	public static readonly WorldPosition Empty = new(null, null, null);

	public bool Exists => this.PosX is not null && this.PosY is not null;
	public static implicit operator bool(WorldPosition? position) => position?.Exists is true;

	public static implicit operator Vector2(WorldPosition? pos) => new(pos?.PosX ?? 0, pos?.PosY ?? 0);
	public static implicit operator WorldPosition(Vector2? pos) => new(pos?.X ?? 0, pos?.Y ?? 0, null);

	public static implicit operator WorldPosition(GameObject? thing) => thing is not null ? new(thing.Position.X, thing.Position.Y, thing.Position.Z) : Empty;

	[MoonSharpHidden]
	public Vector3 GameEnginePosition => new(this?.PosX ?? 0, this?.PosZ ?? 0, this?.PosY ?? 0);

	public float? FlatDistanceFrom(IWorldObjectWrapper? other) => this && other?.Exists is true
		? Vector2.Distance(this, other.Position)
		: null;
	public float? FlatDistance => this.FlatDistanceFrom((WorldPosition)Service.ClientState.LocalPlayer);

	public float? DistanceFrom(IWorldObjectWrapper? other) => this && other?.Exists is true
		? Vector3.Distance(this.GameEnginePosition, other!.Position.GameEnginePosition)
		: null;
	public float? Distance => this.DistanceFrom((WorldPosition)Service.ClientState.LocalPlayer);

	internal Vector3? UiCoords {
		get {
			if (!this)
				return null;
			uint zone = Service.ClientState.TerritoryType;
			if (zone > 0) {
				Map? map = Service.DataManager.GetExcelSheet<Map>()!.GetRow(zone);
				TerritoryTypeTransient? territoryTransient = Service.DataManager.GetExcelSheet<TerritoryTypeTransient>()!.GetRow(zone);
				if (map is not null && territoryTransient is not null) {
					return MapUtil.WorldToMap(this.GameEnginePosition, map, territoryTransient, true);
				}
			}
			return null;
		}
	}
	public float? MapX => this?.UiCoords?.X;
	public float? MapY => this?.UiCoords?.Y;
	public float? MapZ => this?.UiCoords?.Z;

	public override string ToString() => $"(x:{this.MapX}, y:{this.MapY}" + (this.MapZ is float z ? $", z:{z}" : "") + ")";

	[MoonSharpHidden] // implemented only because it's necessary for the IWorldObjectWrapper interface
	public WorldPosition Position => this;

	public int CompareTo(WorldPosition? other) {
		if (ReferenceEquals(this, other))
			return 0;
		if (other is null)
			return 1;
		float distA, distB;
		if (this.PosZ is not null && other.PosZ is not null) {
			distA = this.Distance!.Value;
			distB = other.Distance!.Value;
		}
		else {
			distA = this.FlatDistance!.Value;
			distB = other.FlatDistance!.Value;
		}
		return distA < distB
			? -1
			: distA > distB
			? 1
			: 0;
	}

	public static bool operator <(WorldPosition left, WorldPosition right) => left is null ? right is not null : left.CompareTo(right) < 0;

	public static bool operator <=(WorldPosition left, WorldPosition right) => left is null || left.CompareTo(right) <= 0;

	public static bool operator >(WorldPosition left, WorldPosition right) => left is not null && left.CompareTo(right) > 0;

	public static bool operator >=(WorldPosition left, WorldPosition right) => left is null ? right is null : left.CompareTo(right) >= 0;
}
