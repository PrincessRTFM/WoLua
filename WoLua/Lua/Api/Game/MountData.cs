namespace PrincessRTFM.WoLua.Lua.Api.Game;

using System;

using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

[MoonSharpUserData]
public sealed record class MountData: IEquatable<MountData> {
	public readonly bool Active;
	public readonly ushort Id;
	public readonly string? Name;
	public readonly string? LowercaseArticle;
	public readonly string? UppercaseArticle;
	public MountData(ushort id) {
		Mount? row = id > 0 ? Service.DataManager.GetExcelSheet<Mount>()!.GetRow(id) : null;

		this.Active = row is not null;
		this.Id = this.Active ? id : (ushort)0;
		this.Name = row?.Singular?.RawString;
		this.UppercaseArticle = row is not null ? "A" + (row.StartsWithVowel > 0 ? "n" : string.Empty) : null;
		this.LowercaseArticle = this.UppercaseArticle?.ToLower();
	}

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => this.Name ?? string.Empty;
}
