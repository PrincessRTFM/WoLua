# WoLua Game API
_How do you make a game script that can't touch the game?_

## Usage
The Game API is top-level, which means it's accessible through the global `Game`.

## Properties
The following properties exist on the `Game` API object.

- `Player`, API\
  Access to the [Player API](player.md).

- `Chocobo`, API\
  Access to the [Chocobo API](chocobo.md).

- `Toast`, API\
  Access to the [Toast API](toast.md).

- `EorzeanTime`, readonly object\
  The current Eorzean time. The returned object has only two properties, `Hours` and `Minutes`. Both should be self-explanatory.

- `Weather`, readonly object\
  The current [weather](weather.md) for the current zone.

- `NearbyEntities`, readonly enumerable\
  A sequence of [entity wrappers](entity.md) for nearby game entities. Only objects close enough for your client to load will be available. In areas of sufficient congestion, your client may not load everything, in which case unloaded entities will be missing from this list. Order is neither specified nor guaranteed, as this is read from the game's memory.

- `Fates`, readonly enumerable\
  A sequence of [FATE wrappers](fate.md) for all FATEs in your current zone. Only FATEs that exist in the world will be returned, including those not yet started but available to begin. Order is neither specified nor guaranteed, as this is read from the game's memory.

## Methods
The following methods are avilable on the `Game` API object.

- `boolean|nil PlaySoundEffect(int)`\
  Plays one of the sixteen `<se.##>` game sound effects, without printing anything to the user's chat. The ID must be in the range of `1` to `16` (inclusive) or this will silently fail, with particular emphasis on _silently_.\
  Returns `true` if the given ID was valid, `false` if it wasn't, or `nil` if there was an error. This is so that `if not Game.PlaySoundEffect(id) then` will always execute if no sound was played.

- `nil PrintMessage(any...)`\
  Prints the given values to the chatlog, concatenated by one space. Special values will be given more useful representations, such as tables rendering as JSON.

- `nil PrintError(any...)`\
  Prints the given values to the chatlog in red, concatenated by one space. Special values will be given more useful representations, such as tables rendering as JSON.

- `nil SendChat(string)`\
  Treats the given string as if the user had typed it into their chatlog input, except that it doesn't end up in their message history. **This is dangerous** because it can also send plain chat in whatever the user's current chat channel is! Do not accept untrusted input without carefully checking it over!

- `EntityWrapper FindNearestEntity(string)`\
  Searches all nearby entities for those whose name _exactly_ matches that provided, and returns the nearest if any are found. If none are found, the returned entity wrapper represents nothing.

- `FATEWrapper FindFate(string)`\
  Searches all FATEs in your current zone for any whose name _exactly_ matches that provided, and returns it if one is found. If none are found, the returned FATE wrapper represents nothing. If more than one FATE somehow exists with the same name, the one _nearest_ to you is returned.
