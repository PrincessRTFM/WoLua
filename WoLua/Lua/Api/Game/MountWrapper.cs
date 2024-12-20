using System;
using System.Collections.Generic;

using Lumina.Excel;
using Lumina.Excel.Sheets;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Equals))]
public sealed record class MountWrapper: IEquatable<MountWrapper> { // TODO luadoc all of this
	internal static readonly Dictionary<ushort, string> mountNames = [];
	internal static readonly Dictionary<ushort, string> mountArticles = [];
	internal static void LoadGameData() {
		using MethodTimer logtimer = new();

		ExcelSheet<Mount> mounts = Service.DataManager.GetExcelSheet<Mount>()!;
		foreach (Mount mount in mounts) {
			mountNames[(ushort)mount.RowId] = mount.Singular.ToString();
			mountArticles[(ushort)mount.RowId] = "A" + (mount.StartsWithVowel > 0 ? "n" : string.Empty);
		}
		mountNames.Remove(0);
		mountArticles.Remove(0);
	}

	public bool Active { get; }
	public ushort Id { get; }
	public string? Name { get; }
	public string? LowercaseArticle { get; }
	public string? UppercaseArticle { get; }

	public MountWrapper(ushort id) {
		this.Active = mountNames.TryGetValue(id, out string? name);
		this.Id = this.Active ? id : (ushort)0;
		this.Name = name;
		this.UppercaseArticle = this.Active ? mountArticles[id] : null;
		this.LowercaseArticle = this.UppercaseArticle?.ToLower();
	}

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => this.Name ?? string.Empty;
}
