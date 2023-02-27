namespace PrincessRTFM.WoLua.Lua.Actions;

using System;

public class PauseAction: ScriptAction {
	public readonly uint Delay;

	internal PauseAction(uint ms) {
		this.Delay = ms;
	}

	protected override void Process(ScriptContainer script) {
		script.log($"{this.Delay}ms", "PAUSE");
		script.ActionQueue.ActionThreshold = DateTime.Now.AddMilliseconds(this.Delay);
	}

	public override string ToString()
		=> $"Delay({this.Delay}ms)";

}
