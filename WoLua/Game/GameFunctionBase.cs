namespace PrincessRTFM.WoLua.Game;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Dalamud.Hooking;
using Dalamud.Logging;

using PrincessRTFM.WoLua.Constants;

public abstract class GameFunctionBase<T> where T : Delegate {
	private readonly IntPtr addr = IntPtr.Zero;
	public IntPtr Address => this.addr;
	private T? function;
	public bool Valid => this.function is not null || this.Address != IntPtr.Zero;
	public T? Delegate {
		get {
			if (this.function is not null)
				return this.function;
			if (this.Address != IntPtr.Zero) {
				this.function = Marshal.GetDelegateForFunctionPointer<T>(this.Address);
				return this.function;
			}
			PluginLog.Error($"[{LogTag.PluginCore}] {this.GetType().Name} invocation FAILED: no pointer available");
			return null;
		}
	}
	internal GameFunctionBase(string sig, int offset = 0) {
		if (Service.Scanner.TryScanText(sig, out this.addr)) {
			this.addr += offset;
			ulong totalOffset = (ulong)this.Address.ToInt64() - (ulong)Service.Scanner.Module.BaseAddress.ToInt64();
			PluginLog.Information($"[{LogTag.PluginCore}] {this.GetType().Name} loaded; address = 0x{this.Address.ToInt64():X16}, base memory offset = 0x{totalOffset:X16}");
		}
		else {
			PluginLog.Warning($"[{LogTag.PluginCore}] {this.GetType().Name} FAILED, could not find address from signature: ${sig.ToUpper()}");
		}
	}
	[SuppressMessage("Reliability", "CA2020:Prevent from behavioral change", Justification = "If this explodes, we SHOULD be throwing")]
	internal GameFunctionBase(IntPtr address, int offset = 0) {
		this.addr = address + offset;
		ulong totalOffset = (ulong)this.Address.ToInt64() - (ulong)Service.Scanner.Module.BaseAddress.ToInt64();
		PluginLog.Information($"[{LogTag.PluginCore}] {this.GetType().Name} loaded; address = 0x{this.Address.ToInt64():X16}, base memory offset = 0x{totalOffset:X16}");
	}

	public dynamic? Invoke(params dynamic[] parameters)
		=> this.Delegate?.DynamicInvoke(parameters);

	public Hook<T> Hook(T handler)
		=> Hook<T>.FromAddress(this.Address, handler);
}
