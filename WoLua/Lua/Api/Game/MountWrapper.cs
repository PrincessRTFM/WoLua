using System;
using System.Collections.Generic;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Equals))]
public sealed record class MountWrapper: IEquatable<MountWrapper> { // TODO luadoc all of this
	internal static readonly Dictionary<ushort, string> mountNames = new();
	internal static readonly Dictionary<ushort, string> mountArticles = new();
	internal static void LoadGameData() {
		using MethodTimer logtimer = new();

		ExcelSheet<Mount> mounts = Service.DataManager.GetExcelSheet<Mount>()!;
		foreach (Mount mount in mounts) {
			mountNames[(ushort)mount.RowId] = mount.Singular;
			mountArticles[(ushort)mount.RowId] = "A" + (mount.StartsWithVowel > 0 ? "n" : string.Empty);
		}
	}

	public bool Active { get; }
	public ushort Id { get; }
	public string? Name { get; }
	public string? LowercaseArticle { get; }
	public string? UppercaseArticle { get; }

	public MountWrapper(ushort id) {
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
