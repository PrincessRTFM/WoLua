using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Docs;
using PrincessRTFM.WoLua.Ui.Chat;

namespace PrincessRTFM.WoLua.Lua.Api;

public abstract class ApiBase: IDisposable {
	private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	[LuaDoc("If this is `true`, the API has been disposed of and **MUST NOT** be used.",
		"This should never happen in a running script, but be careful when using coroutines.")]
	public bool Disposed { get; protected set; } = false;

	protected internal virtual void PreInit() { }
	protected internal virtual void Init() { }
	protected internal virtual void PostInit() { }

	[MoonSharpHidden]
	public ScriptContainer Owner { get; private set; }

	[MoonSharpHidden]
	public string DefaultMessageTag { get; init; }

	private readonly PropertyInfo[] disposables;
	private readonly MemberInfo[] wipeOnDispose;

	[MoonSharpHidden]
	public ApiBase(ScriptContainer source) {
		Type me = this.GetType();
		Type apiBase = typeof(ApiBase);
		Type disposable = typeof(IDisposable);
		this.Owner = source;
		this.DefaultMessageTag = me.Name.ToUpper();
		this.disposables = me
			.GetProperties(AllInstance)
			.Where(p => p.PropertyType.IsAssignableTo(disposable) && p.CanRead)
			.ToArray();
		this.wipeOnDispose = me
			.GetProperties(AllInstance)
			.Where(p => p.CanWrite)
			.Where(p => p.GetCustomAttribute<WipeOnDisposeAttribute>()?.Value is true)
			.ToArray();

		IEnumerable<PropertyInfo> autoAssign = me
			.GetProperties(bindingAttr: AllInstance)
			.Where(p => p.CanRead && p.CanWrite && !p.PropertyType.IsAbstract && p.PropertyType.IsAssignableTo(apiBase) && p.GetValue(this) is null);
		Type[] ctorArgTypes = new Type[] { typeof(ScriptContainer) };
		object?[] ctorArgs = new object?[] { this.Owner };
		foreach (PropertyInfo p in autoAssign) {
			ConstructorInfo? ctor = p.PropertyType.GetConstructor(AllInstance, ctorArgTypes);
			if (ctor is null)
				continue;
			if (ctor.Invoke(ctorArgs) is not ApiBase inject)
				continue;
			p.SetValue(this, inject);
			this.Log($"Automatically injected {inject.GetType().Name} into {p.DeclaringType?.Name ?? me.Name}.{p.Name}", LogTag.ScriptLoader, true);
		}
	}

	protected void Log(string message, string? tag = null, bool force = false) {
		if (this.Disposed || this.Owner.Disposed)
			return;

		this.Owner.Log(message, tag ?? this.DefaultMessageTag, force);
	}
	protected void DeprecationWarning(string? alternative = null) {
		StackFrame frame = new(1, true);
		MethodBase? method = frame.GetMethod();
		if (method is null) {
			Service.Log.Warning("Failed to get MethodBase for caller of ApiBase.DeprecationWarning()");
			return;
		}
		string owner = method.DeclaringType?.Name ?? "<unknown API>";
		string name = method.Name;
		string descriptor;
		string action;
		if (name.StartsWith("get_") || name.StartsWith("set_")) {
			name = name[4..];
			descriptor = $"{owner}.{name}";
			action = name.StartsWith("get_") ? "read from" : "written to";
		}
		else {
			string args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
			descriptor = $"{owner}.{name}({args})";
			action = "called";
		}
		this.Log($"Deprecated API member {descriptor} {action}, issuing a warning to the user", LogTag.DeprecatedApiMember, true);
		string message = $"{descriptor} is deprecated and should not be used.";
		if (!string.IsNullOrWhiteSpace(alternative))
			message += $" Please use {alternative} instead.";
		Service.Plugin.Print(message, Foreground.Error, this.Owner.PrettyName);
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

		this.Owner.Log(this.GetType().Name, LogTag.Dispose, true);

		foreach (PropertyInfo disposable in this.disposables) {
			(disposable.GetValue(this) as IDisposable)?.Dispose();
			if (disposable.CanWrite)
				disposable.SetValue(this, null);
		}

		foreach (MemberInfo item in this.wipeOnDispose) {
			(item as PropertyInfo)?.SetValue(this, null);
			(item as FieldInfo)?.SetValue(this, null);
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
