namespace PrincessRTFM.WoLua.Lua.Docs;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
internal class LuaDocAttribute: Attribute {
	public string Description { get; private init; }
	public string[] Lines => this.Description.Split('\n', StringSplitOptions.TrimEntries);

	public LuaDocAttribute(string help) {
		this.Description = help;
	}
}
