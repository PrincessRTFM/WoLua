using System;

namespace PrincessRTFM.WoLua.Lua.Docs;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
internal class AsLuaTypeAttribute: Attribute {
	public string LuaName { get; }

	public AsLuaTypeAttribute(string luaType) => this.LuaName = luaType;
	public AsLuaTypeAttribute(LuaType luaType) : this(luaType.LuaName()) { }
}
