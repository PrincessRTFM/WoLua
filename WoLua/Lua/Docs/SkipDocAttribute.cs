namespace PrincessRTFM.WoLua.Lua.Docs;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
internal class SkipDocAttribute: Attribute {
	public SkipDocAttribute(string justification) {
		_ = justification;
	}
}
