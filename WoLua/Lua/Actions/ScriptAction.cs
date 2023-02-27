namespace PrincessRTFM.WoLua.Lua.Actions;

public abstract class ScriptAction {
	public void Run(ScriptContainer script) {
		if (script is null || script.Disposed)
			return;

		this.Process(script);
	}
	protected abstract void Process(ScriptContainer script);

	public override string ToString()
		=> this.GetType().Name;
}
