using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

using Dalamud.Game.ClientState.Objects.Types;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Game;
using PrincessRTFM.WoLua.Lua.Api.Game;
using PrincessRTFM.WoLua.Lua.Docs;
using PrincessRTFM.WoLua.Ui.Chat;

namespace PrincessRTFM.WoLua.Lua.Api;

// This API is for everything pertaining to the actual game, including holding more specific APIs.
[MoonSharpUserData]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
public class GameApi: ApiBase {

	#region Initialisation

	[MoonSharpHidden]
	internal GameApi(ScriptContainer source) : base(source) { }

	#endregion

	#region Sub-APIs

	public PlayerApi Player { get; private set; } = null!;
	public ChocoboApi Chocobo { get; private set; } = null!;
	public ToastApi Toast { get; private set; } = null!;
	public DalamudApi Dalamud { get; private set; } = null!;

	#endregion

	#region Chat

	[LuaDoc($"Prints a message into the user's local chat log using {Plugin.Name}'s default colour.",
		$"The message will be automatically prefixed with `[{Plugin.Name}]` and the script's name for clarity.")]
	public void PrintMessage([AsLuaType(LuaType.Any), LuaDoc("Multiple values will be concatenated with a single space")] params DynValue[] messages) {
		if (this.Disposed)
			return;
		if (messages.Length == 0)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, null, this.Owner.PrettyName);
	}

	[LuaDoc("Prints a message into the user's local chat log in red.",
		$"The message will be automatically prefixed with `[{Plugin.Name}]` and the script's name for clarity.")]
	public void PrintError([AsLuaType(LuaType.Any), LuaDoc("Multiple values will be concatenated with a single space")] params DynValue[] messages) {
		if (this.Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
		this.Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, Foreground.Error, this.Owner.PrettyName);
	}

	[LuaDoc("Sends text to the game as if the user had typed it into their chat box themselves.",
		"***THIS IS DANGEROUS.***",
		"If the text to be sent does NOT start with a '/' then it will be treated as a **plain chat message**. USE **EXTREME** CAUTION.")]
	public void SendChat(string chatline) {
		if (this.Disposed)
			return;

		string cleaned = Service.Common.Functions.Chat.SanitiseText(chatline);
		if (!string.IsNullOrWhiteSpace(cleaned)) {
			this.Log(cleaned, LogTag.ServerChat);
			Service.Common.Functions.Chat.SendMessage(cleaned);
		}
	}
	#endregion

	#region Object table

	[LuaDoc("Iterates over all real and not-dead entities near the player.",
		"Only objects close enough to be loaded by the client will be seen.",
		"Order is neither specified nor guaranteed.")]
	public IEnumerable<EntityWrapper> NearbyEntities => Service.Objects.Where(o => GameObject.IsValid(o) && o.IsTargetable && !o.IsDead).Select(o => new EntityWrapper(o)).ToList();

	[LuaDoc("Finds the nearest (three-dimensional distance) real and not-dead entity with the given (case-sensitive) name.",
		"If no such entity could be found, the returned EntityWrapper will point to nothing.",
		"This is an intensive method, since it must examine EVERY nearby game object to find matches, then identify the nearest one. Please cache the return value during execution frames.")]
	public EntityWrapper FindNearestEntity(string name) {
		this.Log($"Searching object table for name: {name}", LogTag.ObjectTable);
		return this.NearbyEntities
			.Where(o => o.Name == name)
			.OrderBy(e => e.Distance)
			.FirstOrDefault(EntityWrapper.Empty);
	}

	#endregion

	[LuaDoc("Plays one of the sixteen `<se.##>` sound effects without printing anything to the user's chat.",
		"Using an ID that isn't in the range of 1-16 (inclusive) will silently fail. With an emphasis on silently.")]
	[return: LuaDoc("`true` if the provided sound effect ID was a valid sound, `false` if it wasn't, or `nil` if there was an internal error")]
	public bool? PlaySoundEffect(int id) {
		if (this.Disposed)
			return null;

		if (!Service.Sounds.Valid)
			return null;
		Sound sound = SoundsExtensions.FromGameIndex(id);
		if (sound.IsSound())
			Service.Sounds.Play(sound);
		return sound.IsSound();
	}

	// TODO map flags?
	// TODO allow examining the FATE table directly (would allow effectively recreating TinyCmd's `/fate` command)
	// TODO allow checking game settings via Service.GameConfig
	// TODO allow accessing job gauge data via Service.JobGauges

}
