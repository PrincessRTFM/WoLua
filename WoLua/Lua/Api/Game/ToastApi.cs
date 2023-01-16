namespace PrincessRTFM.WoLua.Lua.Api.Game;

using Dalamud.Game.Gui.Toast;

public class ToastApi: ApiBase {
	internal ToastApi(ScriptContainer source) : base(source, "TOAST") { }

	public void Short(string text) {
		if (this.Disposed)
			return;

		Service.Toast.ShowNormal(text, new ToastOptions() { Speed = ToastSpeed.Fast });
	}
	public void Long(string text) {
		if (this.Disposed)
			return;

		Service.Toast.ShowNormal(text, new ToastOptions() { Speed = ToastSpeed.Slow });
	}

	public void Error(string text) {
		if (this.Disposed)
			return;

		Service.Toast.ShowError(text);
	}

	public void TaskComplete(string text, bool silent) {
		if (this.Disposed)
			return;

		Service.Toast.ShowQuest(text, new QuestToastOptions() { DisplayCheckmark = true, PlaySound = !silent });
	}
	public void TaskComplete(string text, uint icon, bool silent) {
		if (this.Disposed)
			return;

		Service.Toast.ShowQuest(text, new QuestToastOptions() { IconId = icon, PlaySound = !silent });
	}
	public void TaskComplete(string text)
		=> this.TaskComplete(text, false);
	public void TaskComplete(string text, uint icon)
		=> this.TaskComplete(text, icon, false);

}
