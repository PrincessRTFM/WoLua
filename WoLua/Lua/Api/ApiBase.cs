namespace PrincessRTFM.WoLua.Lua.Api;

using System;
using System.Linq;
using System.Reflection;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Constants;

public abstract class ApiBase: IDisposable {
	public bool Disposed { get; protected set; } = false;

	[MoonSharpHidden]
	public ScriptContainer Owner { get; private set; }

	[MoonSharpHidden]
	public string DefaultMessageTag { get; init; }

	private readonly PropertyInfo[] disposables;

	[MoonSharpHidden]
	public ApiBase(ScriptContainer source) {
		this.Owner = source;
		this.DefaultMessageTag = this.GetType().Name.ToUpper();
		Type disposable = typeof(IDisposable);
		this.disposables = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.IsAssignableTo(disposable) && p.CanRead).ToArray();
	}

	protected void Log(string message, string? tag = null, bool force = false) {
		if (this.Disposed || this.Owner.Disposed)
			return;

		this.Owner.log(message, tag ?? this.DefaultMessageTag, force);
	}

	protected internal static string ToUsefulString(DynValue value, bool typed = false)
		=> (typed ? $"{value.Type}: " : "")
		+ value.Type switch {
			//DataType.Nil => throw new System.NotImplementedException(),
			DataType.Void => value.ToDebugPrintString(),
			//DataType.Boolean => throw new System.NotImplementedException(),
			//DataType.Number => throw new System.NotImplementedException(),
			//DataType.String => throw new System.NotImplementedException(),
			DataType.Function => $"luafunc #{value.Function.ReferenceID} @ 0x{value.Function.EntryPointByteCodeLocation:X8}",
			DataType.Table => value.Table.TableToJson(),
			DataType.Tuple => value.ToDebugPrintString(),
			DataType.UserData => $"userdata[{value.UserData.Object?.GetType()?.FullName ?? "<static>"}] {value.ToDebugPrintString()}",
			DataType.Thread => value.ToDebugPrintString(),
			DataType.ClrFunction => $"function {value.Callback.Name}",
			DataType.TailCallRequest => value.ToDebugPrintString(),
			DataType.YieldRequest => value.ToDebugPrintString(),
			_ => value.ToPrintString(),
		};

	#region Metamethods
#pragma warning disable CA1822 // Mark members as static - MoonSharp only inherits metamethods if they're non-static

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => $"nil[{this.GetType().FullName}]";

	[MoonSharpUserDataMetamethod(Metamethod.Concatenate)]
	public string MetamethodConcat(string left, ApiBase right) => $"{left}{right}";
	[MoonSharpUserDataMetamethod(Metamethod.Concatenate)]
	public string MetamethodConcat(ApiBase left, string right) => $"{left}{right}";
	[MoonSharpUserDataMetamethod(Metamethod.Concatenate)]
	public string MetamethodConcat(ApiBase left, ApiBase right) => $"{left}{right}";

#pragma warning restore CA1822 // Mark members as static
	#endregion

	#region IDisposable
	protected virtual void Dispose(bool disposing) {
		if (this.Disposed)
			return;
		this.Disposed = true;

		this.Owner.log(this.GetType().Name, LogTag.Dispose, true);

		foreach (PropertyInfo disposable in this.disposables) {
			(disposable.GetValue(this) as IDisposable)?.Dispose();
			if (disposable.CanWrite)
				disposable.SetValue(this, null);
		}

		this.Owner = null!;
	}

	~ApiBase() {
		this.Dispose(false);
	}

	[MoonSharpHidden]
	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
