namespace PrincessRTFM.WoLua.Lua.Api.Game;

using Dalamud.Memory;

using FFXIVClientStructs.FFXIV.Client.Game.UI;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

[MoonSharpUserData]
public class ChocoboApi: ApiBase {
	[MoonSharpHidden]
	internal ChocoboApi(ScriptContainer source) : base(source) { }
	private unsafe Buddy? obj {
		get {
			UIState* ui = UIState.Instance();
			return ui is null ? null : ui->Buddy;
		}
	}
	public static implicit operator bool(ChocoboApi? bird) => bird?.obj is not null;

	public float? TimeLeft => this.obj?.TimeLeft;
	public bool? Summoned => this ? (this.TimeLeft ?? 0) > 0 : null;
	public uint? CurrentXP => this.obj?.CurrentXP;
	public byte? Rank => this.obj?.Rank;
	public bool? Unlocked => this ? (this.Rank ?? 0) > 0 : null;
	public byte? Stars => this.obj?.Stars;
	public byte? SkillPoints => this.obj?.SkillPoints;
	public byte? DefenderLevel => this.obj?.DefenderLevel;
	public byte? AttackerLevel => this.obj?.AttackerLevel;
	public byte? HealerLevel => this.obj?.HealerLevel;
	public unsafe string? Name {
		get {
			if (this.obj is null)
				return null;
			Buddy bird = this.obj.Value;
			return MemoryHelper.ReadSeStringNullTerminated((nint)bird.Name).TextValue;
		}
	}

	public uint? CurrentHp => (this.Summoned ?? false) ? (this.obj?.Companion)?.CurrentHealth : null;
	public uint? MaxHp => (this.Summoned ?? false) ? (this.obj?.Companion)?.MaxHealth : null;

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => this.Name ?? string.Empty;
}
