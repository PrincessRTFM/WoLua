namespace PrincessRTFM.WoLua.Lua.Docs;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
internal class AsLuaTypeAttribute: Attribute {
	public string LuaName { get; }

	public AsLuaTypeAttribute(string luaType) {
		this.LuaName = luaType;
	}
	public AsLuaTypeAttribute(LuaType luaType): this(luaType.LuaName()) { }
}
