namespace PrincessRTFM.WoLua.Lua;

using System;

[AttributeUsage(AttributeTargets.Property)]
internal class LuaGlobalAttribute: Attribute {
	public readonly string Name;
	public LuaGlobalAttribute(string name) {
		this.Name = name;
	}
}
