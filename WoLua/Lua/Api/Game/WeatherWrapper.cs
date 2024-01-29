using System;
using System.Collections.Generic;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Docs;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
public sealed record class WeatherWrapper: IEquatable<WeatherWrapper> {
	internal static readonly Dictionary<uint, string> weatherNames = new() {
		{ 0, "unknown" },
	};
	internal static readonly Dictionary<uint, string> weatherDescriptions = new() {
		{ 0, "invalid weather" },
	};

	[LuaDoc("The raw (internal) numeric (unsigned integer) ID of the weather this wrapper represents.")]
	public uint Id { get; init; }

	[LuaDoc("The player-friendly name of this weather, in Title Case. This is what's shown when you hover over the weather icon on your minimap.")]
	public string Name { get; init; }

	[LuaDoc("A short description of this weather, (mostly) suitable for RP-style usage.",
		"Some special weathers may sound strange.")]
	public string Description { get; init; }

	[LuaDoc("Whether this object represents a real type of weather (with an actual name and description).")]
	public bool Valid => this.Id > 0 && !string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(this.Description);
	public static implicit operator bool(WeatherWrapper? wrapper) => wrapper?.Valid ?? false;

	[LuaDoc("The player-friendly name of this weather, except in lowercase.")]
	public override string ToString() => this.Name.ToLower();

	public WeatherWrapper(uint id) {
		this.Id = id;
		this.Name = weatherNames.TryGetValue(this.Id, out string? name) ? name : string.Empty;
		this.Description = weatherDescriptions.TryGetValue(this.Id, out string? desc) ? desc : string.Empty;
	}

	#region Initialisation

	internal static void LoadGameData() {
		using MethodTimer logtimer = new();

		Service.Log.Information($"[{LogTag.Weather}] Initialising API data");

		Service.Log.Information($"[{LogTag.Weather}] Loading weather types");
		if (Service.DataManager.GetExcelSheet<Weather>() is ExcelSheet<Weather> skies) {
			// Cache the names and descriptions for each type of weather
			foreach (Weather row in skies) {
				weatherNames[row.RowId] = row.Name;
				weatherDescriptions[row.RowId] = row.Description;
			}
			Service.Log.Information($"[{LogTag.Weather}] Indexed {weatherNames.Count} weather types");
		}
		else {
			Service.Log.Fatal($"[{LogTag.Weather}] Failed to retrieve data sheet for weather!");
		}

	}

	#endregion

}
