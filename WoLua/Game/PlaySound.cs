namespace PrincessRTFM.WoLua.Game;

public delegate ulong SoundFunc(int soundId, ulong unknown1, ulong unknown2);

public class PlaySound: GameFunctionBase<SoundFunc> {
	internal PlaySound() : base("E8 ?? ?? ?? ?? 4D 39 BE") { }
	public void Play(Sound sound) {
		if (this.Valid && sound.IsSound()) {
			Service.Log.Verbose($"Playing sound {sound.ToSoundName()}");
			this.Invoke((int)sound, 0ul, 0ul);
		}
		else if (this.Valid) {
			Service.Log.Warning($"Something tried to play an invalid sound effect");
		}
		else {
			Service.Log.Warning("Unable to play sounds, game function couldn't be located");
		}
	}
}
