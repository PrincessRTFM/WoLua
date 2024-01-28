using System;

using PrincessRTFM.WoLua.Constants;

namespace PrincessRTFM.WoLua.Lua.Actions;

public class PauseAction: ScriptAction {
	public uint Delay { get; }

	internal PauseAction(uint ms) => this.Delay = ms;

	protected override void Process(ScriptContainer script) {
		script.Log($"{this.Delay}ms", LogTag.ActionPause);
		script.ActionQueue.ActionThreshold = DateTime.Now.AddMilliseconds(this.Delay);
	}

	public override string ToString()
		=> $"Delay({this.Delay}ms)";

}
