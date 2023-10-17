namespace PrincessRTFM.WoLua.Lua.Api;

using System.Linq;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Game;
using PrincessRTFM.WoLua.Lua.Api.Game;
using PrincessRTFM.WoLua.Lua.Docs;
using PrincessRTFM.WoLua.Ui.Chat;

// This API is for everything pertaining to the actual game, including holding more specific APIs.
[MoonSharpUserData]
public class GameApi: ApiBase {

	#region Initialisation

	[MoonSharpHidden]
	internal GameApi(ScriptContainer source) : base(source) { }

	#endregion

	#region Sub-APIs

	public PlayerApi Player { get; private set; } = null!;
	public ChocoboApi Chocobo { get; private set; } = null!;
	public ToastApi Toast { get; private set; } = null!;
	public DalamudApi Dalamud { get; private set; } = null!;

	#endregion

	#region Chat

	[LuaDoc("Prints a message into the user's local chat log using the normal default colour")]
	public void PrintMessage([AsLuaType(LuaType.Any), LuaDoc("Multiple values will be concatenated with a single space")] params DynValue[] messages) {
		if (this.Disposed)
			return;
		if (messages.Length == 0)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, null, this.Owner.PrettyName);
	}

	[LuaDoc("Prints a message into the user's local chat log in red")]
	public void PrintError([AsLuaType(LuaType.Any), LuaDoc("Multiple values will be concatenated with a single space")] params DynValue[] messages) {
		if (this.Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, Foreground.Error, this.Owner.PrettyName);
	}

	[LuaDoc("Sends text to the game as if the user had typed it into their chat box themselves")]
	public void SendChat(string chatline) {
		if (this.Disposed)
			return;

		string cleaned = Service.Common.Functions.Chat.SanitiseText(chatline);
		if (!string.IsNullOrWhiteSpace(cleaned)) {
			this.Log(cleaned, LogTag.ServerChat);
			Service.Common.Functions.Chat.SendMessage(cleaned);
		}
	}
	#endregion

	[LuaDoc("Plays one of the sixteen <se.##> sound effects without printing anything to the user's chat")]
	[return: LuaDoc("true if the provided sound effect ID was a valid sound, false if it wasn't, or nil if there was an internal error")]
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
	// TODO allow examining the object table directly (would allow searching for objects matching criteria, could be useful)
	// TODO allow examining the FATE table directly (would allow effectively recreating TinyCmd's `/fate` command)
	// TODO allow checking game settings via Service.GameConfig
	// TODO allow accessing job gauge data via Service.JobGauges

}
