namespace PrincessRTFM.WoLua.Lua.Api;
using System.Linq;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Lua.Api.Game;
using PrincessRTFM.WoLua.Ui.Chat;

// This API is for everything pertaining to the actual game, including holding more specific APIs.
public class GameApi: ApiBase {
	public const string TAG = "GAME";

	#region Initialisation and IDisposable

	[MoonSharpHidden]
	internal GameApi(ScriptContainer source) : base(source, TAG) {
		this.Player = new(this.Owner);
		this.Toast = new(this.Owner);
	}

	protected override void Dispose(bool disposing) {
		if (this.Disposed)
			return;

		if (disposing) {
			this.Player.Dispose();
			this.Toast.Dispose();
		}

		base.Dispose(disposing);

		this.Player = null!;
		this.Toast = null!;
	}

	#endregion

	public PlayerApi Player { get; private set; }
	public ToastApi Toast { get; private set; }

	public void PrintMessage(params DynValue[] messages) {
		if (this.Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, "CHAT");
		Service.Plugin.Print(message, null, this.ScriptTitle);
	}

	public void PrintError(params DynValue[] messages) {
		if (this.Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, "CHAT");
		Service.Plugin.Print(message, Foreground.Error, this.ScriptTitle);
	}

	public void SendChat(string chatline) {
		if (this.Disposed)
			return;

		string cleaned = Service.Common.Functions.Chat.SanitiseText(chatline);
		if (!string.IsNullOrWhiteSpace(cleaned))
			Service.Common.Functions.Chat.SendMessage(cleaned);
	}

	// TODO map flags, playing sounds?

}
