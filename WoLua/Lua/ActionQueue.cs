using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Dalamud.Plugin.Services;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Actions;

namespace PrincessRTFM.WoLua.Lua;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "It's a queue for script actions")]
public class ActionQueue: IDisposable { // TODO: allow pause/resume via chat command?

	private readonly ConcurrentQueue<ScriptAction> queue = new();
	public DateTime ActionThreshold { get; internal set; } = DateTime.MinValue;
	public ScriptContainer Script { get; private set; }

	public ActionQueue(ScriptContainer source) {
		this.Script = source;
		Service.Framework.Update += this.tick;
	}

	public int Count => this.queue.Count;

	internal void Clear()
		=> this.queue.Clear();
	internal void Add(ScriptAction action)
		=> this.queue.Enqueue(action);

	public bool? PullEvent() {
		if (!Service.ClientState.IsLoggedIn || Service.GameLifecycle.LogoutToken.IsCancellationRequested || Service.GameLifecycle.DalamudUnloadingToken.IsCancellationRequested || Service.GameLifecycle.GameShuttingDownToken.IsCancellationRequested) {
			this.Clear();
			return null;
		}
		if (DateTime.Now < this.ActionThreshold)
			return null;
		if (!this.queue.TryDequeue(out ScriptAction? action))
			return false;
		this.Script.Log(action.ToString(), LogTag.ActionQueue);
		action.Run(this.Script);
		return true;
	}

	private void tick(IFramework framework) => this.PullEvent();

	#region IDisposable
	private bool disposed = false;

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			Service.Framework.Update -= this.tick;
			this.Clear();
		}

		this.Script.Log(this.GetType().Name, LogTag.Dispose, true);

		this.Script = null!;
	}

	~ActionQueue() {
		this.Dispose(false);
	}

	[MoonSharpHidden]
	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
