using System;

namespace PrincessRTFM.WoLua.Lua.Api;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal class WipeOnDisposeAttribute: Attribute {
	public bool Value { get; init; }
	public WipeOnDisposeAttribute(bool value) => this.Value = value;
	public WipeOnDisposeAttribute() : this(true) { }
}
