using System;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "readonly struct")]
[MoonSharpUserData]
public readonly struct EorzeanTime: IComparable<EorzeanTime>, IEquatable<EorzeanTime> {
	public const double ConversionRate = 144d / 7d;

	public readonly byte Hour;
	public readonly byte Minute;

	[MoonSharpHidden]
	public EorzeanTime(byte hour, byte minute) {
		this.Hour = hour;
		this.Minute = minute;
	}
	[MoonSharpHidden]
	public EorzeanTime(long realMs) {
		long totalMinutes = (long)(realMs * ConversionRate / 60000);
		this.Hour = (byte)(totalMinutes / 60 % 24);
		this.Minute = (byte)(totalMinutes % 60);
	}
	[MoonSharpHidden]
	public EorzeanTime() : this(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) { }

	[MoonSharpHidden]
	public override bool Equals(object? obj) => obj is not null && obj is EorzeanTime other && this.Equals(other);

	[MoonSharpHidden]
	public override int GetHashCode() => (this.Hour * 60) + this.Minute;

	[MoonSharpHidden]
	public int CompareTo(EorzeanTime other) {
		return this.Hour > other.Hour
			? 1
			: this.Hour < other.Hour
			? -1
			: this.Minute > other.Minute
			? 1
			: this.Minute < other.Minute
			? -1
			: 0;
	}

	[MoonSharpHidden]
	public bool Equals(EorzeanTime other) => this.CompareTo(other) == 0;

	[MoonSharpHidden]
	public static bool operator ==(EorzeanTime left, EorzeanTime right) => left.Equals(right);

	[MoonSharpHidden]
	public static bool operator !=(EorzeanTime left, EorzeanTime right) => !(left == right);

	[MoonSharpHidden]
	public static bool operator <(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) < 0;

	[MoonSharpHidden]
	public static bool operator <=(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) <= 0;

	[MoonSharpHidden]
	public static bool operator >(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) > 0;

	[MoonSharpHidden]
	public static bool operator >=(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) >= 0;
}
