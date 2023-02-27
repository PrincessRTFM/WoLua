namespace PrincessRTFM.WoLua.Game;

using Dalamud.Logging;

public delegate ulong PlaySoundDelegate(int soundId, ulong unknown1, ulong unknown2);
public class PlaySound: GameFunctionBase<PlaySoundDelegate> {
	internal PlaySound() : base("E8 ?? ?? ?? ?? 4D 39 BE") { }
	public void Play(Sound sound) {
		if (this.Valid && sound.IsSound()) {
			PluginLog.Verbose($"Playing sound {sound.ToSoundName()}");
			this.Invoke((int)sound, 0ul, 0ul);
		}
		else if (this.Valid) {
			PluginLog.Warning($"Something tried to play an invalid sound effect");
		}
		else {
			PluginLog.Warning("Unable to play sounds, game function couldn't be located");
		}
	}
}
