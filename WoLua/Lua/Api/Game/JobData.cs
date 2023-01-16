namespace PrincessRTFM.WoLua.Lua.Api.Game;

using System;

using MoonSharp.Interpreter;

public sealed record class JobData(uint Id, string? Name, string? Abbreviation): IEquatable<JobData> {
	public bool Equals(JobData? other)
		=> this.Id == other?.Id;
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	public string? Abbr
		=> this.Abbreviation;
	public string? ShortName
		=> this.Abbreviation;

	public bool Valid
		=> this.Id > 0 && this.Name is not null && this.Abbreviation is not null;

	public bool IsCrafter
		=> this.Valid && this.Id is >= 8 and <= 15;
	public bool IsGatherer
		=> this.Valid && this.Id is >= 16 and <= 18;
	public bool IsMeleeDPS
		=> this.Valid && this.Id is 2 or 4 or 20 or 22 or 29 or 30 or 34 or 39;
	public bool IsRangedDPS
		=> this.Valid && this.Id is 5 or 23 or 31 or 38;
	public bool IsMagicDPS
		=> this.Valid && this.Id is 7 or 25 or 26 or 27 or 35;
	public bool IsHealer
		=> this.Valid && this.Id is 6 or 24 or 28 or 33 or 40;
	public bool IsTank
		=> this.Valid && this.Id is 3 or 19 or 21 or 32 or 37;

	public bool IsDPS
		=> this.IsMeleeDPS || this.IsRangedDPS || this.IsMagicDPS;

	public bool IsDiscipleOfWar
		=> this.IsMeleeDPS || this.IsRangedDPS || this.IsTank;
	public bool IsDiscipleOfMagic
		=> this.IsMagicDPS || this.IsHealer;

	public bool IsBlu
		=> this.Valid && this.Id is 36;
	public bool IsLimited
		=> this.IsBlu;

	#region Metamathods

	[MoonSharpUserDataMetamethod("__tostring")]
	public override string ToString()
		=> this.Name ?? "Adventurer";

	#endregion
}
