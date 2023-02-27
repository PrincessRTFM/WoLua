namespace PrincessRTFM.WoLua.Lua.Api.Game;

using MoonSharp.Interpreter;

[MoonSharpUserData]
public class PartyApi: ApiBase {
	public PartyApi(ScriptContainer source) : base(source, "PARTY") { }

	public int? Size => this.Owner.GameApi.Player.Loaded ? Service.Party.Length : null;
	public int? Length => this.Size;
	public bool? InAlliance => this.Owner.GameApi.Player.Loaded ? Service.Party.IsAlliance : null;
	public bool? InParty => this.Owner.GameApi.Player.Loaded ? Service.Party.Length > 0 : null;

	public EntityWrapper this[int idx] => new(Service.Party[idx]?.GameObject);
}
