using System;
using System.Threading.Tasks;

namespace PrincessRTFM.WoLua;

public class SingleExecutionTask {
	private readonly Action func;
	protected Task Task { get; private set; } = Task.CompletedTask;

	public bool Running {
		get {
			lock (this) {
				return this.Task.Status is TaskStatus.Running;
			}
		}
	}
	public bool Completed {
		get {
			lock (this) {
				return this.Task.IsCompleted;
			}
		}
	}

	public SingleExecutionTask(Action func) {
		this.func = func;
	}

	public bool TryRun() {
		lock (this) {
			if (this.Completed) {
				this.Task = Task.Run(this.func);
				return true;
			}
		}
		return false;
	}
	public void Run() => this.TryRun();
}
