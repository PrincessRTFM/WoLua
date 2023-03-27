namespace PrincessRTFM.WoLua.Lua.Actions;

using System;
using System.Collections.ObjectModel;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Api;

public class CallbackAction: ScriptAction {
	public DynValue Function { get; }
	private readonly DynValue[] arguments;
	public ReadOnlyCollection<DynValue> Arguments => Array.AsReadOnly(this.arguments);

	public CallbackAction(DynValue callback, params DynValue[] arguments) {
		this.Function = callback;
		this.arguments = arguments;
	}

	protected override void Process(ScriptContainer script) {
		script.log(ApiBase.ToUsefulString(this.Function), LogTag.ActionCallback);
		try {
			script.Engine.Call(this.Function, this.arguments);
		}
		catch (ArgumentException e) {
			Service.Plugin.Error("Error in queued callback function", e, script.PrettyName);
		}
	}

	public override string ToString()
		=> $"Invoke: {ApiBase.ToUsefulString(this.Function, true)}";
}
