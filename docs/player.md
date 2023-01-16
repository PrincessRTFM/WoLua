# WoLua Player API
_Who am I, again?_

## Usage
The Player API is a sub-component of the Game API, which means it's accessible through `Game.Player`.

## Properties
The following properties exist on the `Player` API object.
<!-- SO FUCKING MANY -->

- `Loaded`, readonly boolean
  Indicates whether or not character data is loaded and available. Will be false if not logged in, in which case all of the following properties and methods will return `nil`.

- `CharacterId`, readonly number|nil (unsigned long)
  The current character's unique ID. This does not change even if you change your name or homeworld, so it can be used to associate storage with a particular character.

- `Name`, readonly string|nil
  The current character's name, first and last. Can be combined with `Homeworld` below using `@` as a separator in order to identify a character for sending chat tells.

- `HomeWorldId`, readonly number|nil (unsigned short)
  The internal (FFXIV) numeric ID of the current character's _home_ world. May be useful in niche cases, but you probably want `HomeWorld` instead.

- `CurrentWorldId`, readonly number|nil (unsigned short)
  The internal (FFXIV) numeric ID of the current character's _current_ world. May be useful in niche cases, but you probably want `CurrentWorld` instead.

- `HomeWorld`, readonly string|nil
  The textual name of the current character's _home_ world.

- `CurrentWorld`, readonly string|nil
  The textual name of the current character's _current_ world.

- `Level`, readonly number|nil (unsigned byte)
  The current character's level, which _does_ reflect level syncs.

- `Job`, readonly object
  Exposes the current character's [job data](job.md).

- `Hp`, readonly number|nil (unsigned integer)
  The current character's _current_ HP.

- `MaxHp`, readonly number|nil (unsigned integer)
  The current character's _maximum_ HP at the current level.

- `Mp`, readonly number|nil (unsigned integer)
  The current character's _current_ MP.

- `MaxMp`, readonly number|nil (unsigned integer)
  The current character's _maximum_ MP at the current level.

- `Gp`, readonly number|nil (unsigned integer)
  The current character's _current_ GP.

- `MaxGp`, readonly number|nil (unsigned integer)
  The current character's _maximum_ GP at the current level.

- `Cp`, readonly number|nil (unsigned integer)
  The current character's _current_ CP.

- `MaxCp`, readonly number|nil (unsigned integer)
  The current character's _maximum_ CP at the current level.

- `MapZone`, readonly number|nil (unsigned integer)
  The user's current map zone, used to separate worldspaces. May be zero if the current zone is indeterminate.

- `InCombat`, readonly boolean|nil
  Whether or not the user is currently considered to be in combat by the game.

- `Mounted`, readonly boolean|nil
  Whether or not the user is currently mounted. If `false`, the `MountId` property will be zero.

- `Flying`, readonly boolean|nil
  Whether the user is currently flying on a mount. If you want to know whether the user is in the air but not _flying_ (jumping/falling), use `Jumping` instead.

- `Swimming`, readonly boolean|nil
  Whether the user is currently in a swimming state.

- `Diving`, readonly boolean|nil
  Whether the user is currently diving underwater.

- `Jumping`, readonly boolean|nil
  Whether the user is currently jumping or falling in the air. To see if the user is _flying_, use the `Flying` property instead.

- `Crafting`, readonly boolean|nil
  Whether the user is currently crafting.

- `Gathering`, readonly boolean|nil
  Whether the user is currently gathering.

- `Fishing`, readonly boolean|nil
  Whether the user is currently fishing.

- `Performing`, readonly boolean|nil
  Whether the user is currently performing as a bard.

- `Casting`, readonly boolean|nil
  Whether the user is currently casting something.

- `InCutscene`, readonly boolean|nil
  Whether the user is currently in a cutscene.

- `Trading`, readonly boolean|nil
  Whether the user is currently trading with another player.

- `InDuty`, readonly boolean|nil
  Whether the user is currently in a duty instance.

- `UsingFashionAccessory`, readonly boolean|nil
  Whether the user is currently using a fashion accessory, such as a parasol or wings.

- `WeaponDrawn`, readonly boolean|nil
  Whether the user's weapon is currently drawn.

- `PartyMemberCount`, readonly number|nil (unsigned integer)
  The number of _other_ members of the user's current party. This does not include the player, so a full party will have a `PartyMemberCount` of `7`, not `8`. It also does not include the user's pet (chocobo, carbuncle, etc) if one is present.

- `InAlliance`, readonly boolean|nil
  Whether the user is currently in a multi-party alliance. If this is `false`, you won't be able to use the alliance chat channel.

- `InParty`, readonly boolean|nil
  Whether the user is currently in a party. If this is `false`, you won't be able to use the party chat channel.

- `HasTarget`, readonly boolean|nil
  Whether the user currently has a normal target.

- `HasFocusTarget`, readonly boolean|nil
  Whether the user currently has a focus target.

- `HasMouseoverTarget`, readonly boolean|nil
  Whether the user currently has their cursor over a targetable entity.

- `HasSoftTarget`, readonly boolean|nil
  Whether the user currently has a soft target.

- `MountId`, readonly number|nil (unsigned short)
  The internal (FFXIV) numeric ID of the user's current mount. If the user is not mounted, this will be `0`.

## Methods
The following methods are avilable on the `Player` API object.

- `boolean|nil HasEmote(string)`
  Checks whether the current character has access to the given emote command. Leading slash (`/`) is optional, and _all_ game aliases are accepted, not just the main one. Do not pass a SimpleTweaks alias; they aren't real commands and will not be expanded.
  Returns `true` if you are logged into a character that has access to the given emote, `false` if you are logged into a character that does _not_ have access, or `nil` if you aren't logged in or something breaks.

## Special
When converted to a string (such as by `tostring(Game.Player)`) you will get the current character's name and homeworld in the standard format, or an empty string if you aren't logged in.
