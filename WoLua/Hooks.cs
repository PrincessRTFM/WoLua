using System;

using Dalamud.Game.ClientState.Objects.Types;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace PrincessRTFM.WoLua;

public class Hooks: IDisposable {
	private bool disposed;

	public unsafe IGameObject? UITarget {
		get {
			PronounModule* pronouns = PronounModule.Instance();
			if (pronouns is null)
				return null;
			GameObject* actor = pronouns->UiMouseOverTarget;
			return actor is null ? null : Service.Objects.CreateObjectReference((IntPtr)actor);
		}
	}

	public Hooks() {
		// nop
	}

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			// nop
		}

		// nop
	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
}
