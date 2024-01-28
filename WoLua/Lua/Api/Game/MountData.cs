using System;

using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Equals))]
public sealed record class MountData: IEquatable<MountData> { // TODO luadoc all of this
	public bool Active { get; }
	public ushort Id { get; }
	public string? Name { get; }
	public string? LowercaseArticle { get; }
	public string? UppercaseArticle { get; }

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
