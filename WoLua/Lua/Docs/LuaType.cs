using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;

namespace PrincessRTFM.WoLua.Lua.Docs;

[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "the names are defined externally")]
[Flags]
public enum LuaType: ushort {
	Any = 0,
	String = 1 << 0,
	Boolean = 1 << 1,
	Integer = 1 << 2,
	Number = 1 << 3,
	Table = 1 << 4,
	Function = 1 << 5,
	Userdata = 1 << 6,
	Nil = 1 << 7,
}

public static class LuaTypeExtensions {
	public static string LuaName(this LuaType type, string? userdataName = null) {
		return type is LuaType.Any
			? type.ToString().ToLower()
			: string.Join("|", Enum.GetValues<LuaType>()
				.Where(t => t is not LuaType.Any && type.HasFlag(t))
				.OrderBy(t => (ushort)t)
				.Select(t => t is LuaType.Userdata && !string.IsNullOrEmpty(userdataName) ? userdataName : t.ToString().ToLower())
			);
	}

	public static LuaType FromType(Type originalType) {
		LuaType lua = LuaType.Any;
		Type realType = originalType;
		if (originalType.IsGenericType && realType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
			realType = originalType.GenericTypeArguments[0];
			lua = LuaType.Nil;
		}

		// apparently you can't do a switch using typeof() cases, so here we fuckin' are
		if (realType == typeof(DynValue))
			lua = LuaType.Any;
		else if (realType == typeof(string))
			lua |= LuaType.String;
		else if (realType == typeof(bool))
			lua |= LuaType.Boolean;
		else if (realType == typeof(byte) || realType == typeof(sbyte))
			lua |= LuaType.Integer;
		else if (realType == typeof(short) || realType == typeof(ushort))
			lua |= LuaType.Integer;
		else if (realType == typeof(int) || realType == typeof(uint))
			lua |= LuaType.Integer;
		else if (realType == typeof(long) || realType == typeof(ulong))
			lua |= LuaType.Integer;
		else if (realType == typeof(float) || realType == typeof(double))
			lua |= LuaType.Number;
		else if (realType == typeof(Closure) || realType == typeof(CallbackFunction))
			lua |= LuaType.Function;
		else if (realType == typeof(Table))
			lua |= LuaType.Table;
		else if (realType == typeof(void))
			lua |= LuaType.Nil;
		else if (realType.IsAssignableTo(typeof(IEnumerable)) || (realType.IsGenericType && realType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
			lua |= LuaType.Table;
		else if (realType.IsAssignableTo(typeof(IDictionary)) || (realType.IsGenericType && realType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
			lua |= LuaType.Table;
		else
			lua |= LuaType.Userdata;

		return lua;
	}
}
