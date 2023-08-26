namespace PrincessRTFM.WoLua;

using System;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;

public class Hooks: IDisposable {
	private bool disposed;

	private delegate void UiMouseoverEntityFunc(IntPtr t, IntPtr entity);
	private Hook<UiMouseoverEntityFunc>? uiMouseoverEntity;
	public GameObject? UITarget { get; private set; } = null;
	private void onUiMouseoverEntity(IntPtr self, IntPtr entity) {
		this.uiMouseoverEntity?.Original(self, entity);
		this.UITarget = entity == IntPtr.Zero ? null : Service.Objects.CreateObjectReference(entity);
	}

	public Hooks() {
		nint ptrUiMouseoverEntity = Service.Scanner.ScanModule("E8 ?? ?? ?? ?? 48 8B 5C 24 40 4C 8B 74 24 58 83 FD 02");
		if (ptrUiMouseoverEntity != nint.Zero) {
			int offset = Dalamud.Memory.MemoryHelper.Read<int>(ptrUiMouseoverEntity + 1);
			nint ptrHook = ptrUiMouseoverEntity + 5 + offset;
			this.uiMouseoverEntity = Hook<UiMouseoverEntityFunc>.FromAddress(ptrHook, this.onUiMouseoverEntity);
			this.uiMouseoverEntity.Enable();
		}
	}

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			this.uiMouseoverEntity?.Dispose();
		}

		this.uiMouseoverEntity = null;
	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
}
