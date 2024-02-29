namespace PrincessRTFM.WoLua.Lua.Api.Game;

public interface IWorldObjectWrapper {
	public bool Exists { get; }

	/// <summary>
	/// The raw internal X (east/west) position of this world object
	/// </summary>
	public float? PosX { get; }
	/// <summary>
	/// The raw internal Y (north/south) position of this world object
	/// </summary>
	/// <remarks>
	/// The game engine uses X/Z for horizontal position and Y for vertical, but that's not how positions are displayed to the player, so all of WoLua's APIs reverse those two.
	/// </remarks>
	public float? PosY { get; }
	/// <summary>
	/// The raw internal Z (vertical) position of this world object
	/// </summary>
	/// <remarks>
	/// The game engine uses X/Z for horizontal position and Y for vertical, but that's not how positions are displayed to the player, so all of WoLua's APIs reverse those two.
	/// </remarks>
	public float? PosZ { get; }

	public WorldPosition Position { get; }

	/// <summary>
	/// The player-friendly X (east/west) position of this world object
	/// </summary>
	public float? MapX { get; }
	/// <summary>
	/// The player-friendly Y (north/south) position of this world object
	/// </summary>
	/// <remarks>
	/// The game engine uses X/Z for horizontal position and Y for vertical, but that's not how positions are displayed to the player, so all of WoLua's APIs reverse those two.
	/// </remarks>
	public float? MapY { get; }
	/// <summary>
	/// The player-friendly Z (vertical) position of this world object
	/// </summary>
	/// <remarks>
	/// The game engine uses X/Z for horizontal position and Y for vertical, but that's not how positions are displayed to the player, so all of WoLua's APIs reverse those two.
	/// </remarks>
	public float? MapZ { get; }
}
