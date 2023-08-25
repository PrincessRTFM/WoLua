namespace PrincessRTFM.WoLua.Lua.Api;

using System.Linq;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Game;
using PrincessRTFM.WoLua.Lua.Api.Game;
using PrincessRTFM.WoLua.Ui.Chat;

// This API is for everything pertaining to the actual game, including holding more specific APIs.
[MoonSharpUserData]
public class GameApi: ApiBase {

	#region Initialisation

	[MoonSharpHidden]
	internal GameApi(ScriptContainer source) : base(source) {
		this.Player = new(this.Owner);
		this.Chocobo = new(this.Owner);
		this.Toast = new(this.Owner);
	}

	#endregion

	public PlayerApi Player { get; private set; }
	public ChocoboApi Chocobo { get; private set; }
	public ToastApi Toast { get; private set; }

	public void PrintMessage(params DynValue[] messages) {
		if (this.Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, null, this.Owner.PrettyName);
	}

	public void PrintError(params DynValue[] messages) {
		if (this.Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, Foreground.Error, this.Owner.PrettyName);
	}

	public void SendChat(string chatline) {
		if (this.Disposed)
			return;

		string cleaned = Service.Common.Functions.Chat.SanitiseText(chatline);
		if (!string.IsNullOrWhiteSpace(cleaned)) {
			this.Log(cleaned, LogTag.ServerChat);
			Service.Common.Functions.Chat.SendMessage(cleaned);
		}
	}

	public bool? PlaySoundEffect(int id) {
		if (this.Disposed)
			return null;

		if (!Service.Sounds.Valid)
			return null;
		Sound sound = SoundsExtensions.FromGameIndex(id);
		if (sound.IsSound())
			Service.Sounds.Play(sound);
		return sound.IsSound();
	}

	// TODO map flags?
	// TODO allow examining the object table directly? (would allow searching for objects matching criteria, could be useful)

}
