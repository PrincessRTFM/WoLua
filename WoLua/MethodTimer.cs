namespace PrincessRTFM.WoLua;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Dalamud.Logging;

using PrincessRTFM.WoLua.Constants;

public sealed class MethodTimer: IDisposable {
	private readonly Stopwatch timer;
	private readonly string label;
	public long Elapsed => this.timer.ElapsedMilliseconds;
	public MethodTimer() {
		StackFrame caller = new(1);
		string owner = "????";
		string method = "????";
		string args = "????";
		if (caller.HasMethod()) {
			MethodBase func = caller.GetMethod()!;
			owner = func.DeclaringType?.FullName ?? owner;
			method = func.Name;
			args = string.Join(", ", func.GetParameters().Select(p => p.ParameterType.Name));
		}
		this.label = $"{owner}.{method}({args})";
		this.timer = Stopwatch.StartNew();
	}
	public void Dispose() {
		this.timer.Stop();
#if DEBUG
		PluginLog.Information($"[{LogTag.MethodTiming}] {this.label}: {this.Elapsed}ms");
#endif
	}
}
