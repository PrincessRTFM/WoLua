namespace PrincessRTFM.WoLua.Lua.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal class WipeOnDisposeAttribute: Attribute {
	public bool Value { get; init; }
	public WipeOnDisposeAttribute(bool value) {
		this.Value = value;
	}
	public WipeOnDisposeAttribute(): this(true) { }
}
