using System;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
public record class EorzeanTime: IComparable<EorzeanTime>, IEquatable<EorzeanTime> {
	public const double ConversionRate = 144d / 7d;

	public byte Hour { get; init; }
	public byte Minute { get; init; }

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
	public int CompareTo(EorzeanTime? other) {
		return other is null
			? 1
			: this.Hour > other.Hour
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
	public static bool operator <(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) < 0;

	[MoonSharpHidden]
	public static bool operator <=(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) <= 0;

	[MoonSharpHidden]
	public static bool operator >(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) > 0;

	[MoonSharpHidden]
	public static bool operator >=(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) >= 0;
}
