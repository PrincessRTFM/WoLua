# WoLua Player API
_Who am I, again?_

## Usage
The Player API is a sub-component of the [Game API](game.md), which means it's accessible through `Game.Player`.

## Properties
The following properties exist on the `Player` API object.

- `Loaded`, readonly boolean\
  Indicates whether or not character data is loaded and available. Will be false if not logged in, in which case all of the following properties and methods will return `nil`.

- `CharacterId`, readonly number|nil (unsigned long)\
  The current character's unique ID. This does not change even if you change your name or homeworld, so it can be used to associate storage with a particular character.

- `Name`, readonly string|nil\
  The current character's name, first and last. Can be combined with `.Homeworld` below using `@` as a separator in order to identify a character for sending chat tells.

- `Firstname`, readonly string|nil\
  The current character's first name only.

- `Lastname`, readonly string|nil\
  The current character's last name only.

- `IsMale`, readonly boolean|nil\
  This will be true if the current character is considered male by the game. This is a shortcut for `.Entity.IsMale`.

- `IsFemale`, readonly boolean|nil\
  This will be true if the current character is considered female by the game. This is a shortcut for `.Entity.IsFemale`.

- `IsGendered`, readonly boolean|nil\
  This will be true if the current character is considered to have a gender by the game. This should never be false. This is a shortcut for `.Entity.IsGendered`.

- `HasTitle`, boolean|nil\
  This will be `true` if the current character has a title and `false` if they don't. This is a shortcut for `.Entity.HasTitle`.

- `TitleText`, string|nil\
  This will be the active title of the current character if one is present, or an empty string if not. This is a shortcut for `.Entity.TitleText`.\
  Note that some titles have different forms for male and female characters, such as "God of War" versus "Goddess of War". This property will automatically return the form that is appropriate for the current character.

- `TitleIsPrefix`, boolean|nil\
  This will be `true` if the current character has a title that is a prefix (displays _above_ their name in their nameplate), `false` if their title is a suffix, or `nil` if they do not currently have a title. This is a shortcut for `.Entity.TitleIsPrefix`.

- `Party`, API\
  Access to the [Party API](party.md).

- `Mount`, API\
  Access to the [mount data](mount.md) for the current character.

- `Entity`, readonly object\
  Access to an [entity container](entity.md) representing the current character.

- `Target`, readonly object\
  Access to an [entity container](entity.md) representing the current character's hard target.

- `HasTarget`, readonly boolean|nil\
  Whether the current character _has_ a target. This will be `nil` if you aren't logged in, but is _otherwise_ equivalent to `.Target.Exists`.

- `SoftTarget`, readonly object\
  Access to an [entity container](entity.md) representing the current character's soft target.

- `HasSoftTarget`, readonly boolean|nil\
  Whether the current character _has_ a soft target. This will be `nil` if you aren't logged in, but is _otherwise_ equivalent to `.SoftTarget.Exists`.

- `FocusTarget`, readonly object\
  Access to an [entity container](entity.md) representing the current character's focus target.

- `HasFocusTarget`, readonly boolean|nil\
  Whether the current character _has_ a focus target. This will be `nil` if you aren't logged in, but is _otherwise_ equivalent to `.FocusTarget.Exists`.

- `FieldMouseOverTarget`, readonly object\
  Access to an [entity container](entity.md) representing the current character's _field_ mouseover target. Note that the `O` in `Over` is capitalised.

- `HasFieldMouseOverTarget`, readonly boolean|nil\
  Whether the current character _has_ a field mouseover target. This will be `nil` if you aren't logged in, but is _otherwise_ equivalent to `.FieldMouseOverTarget.Exists`.

- `UiMouseOverTarget`, readonly object\
  Access to an [entity container](entity.md) representing the current character's _UI_ mouseover target. Note that the `O` in `Over` is capitalised.

- `HasUiMouseOverTarget`, readonly boolean|nil\
  Whether the current character _has_ a UI mouseover target. This will be `nil` if you aren't logged in, but is _otherwise_ equivalent to `.UiMouseOverTarget.Exists`.

- `MouseOverTarget`, readonly object\
  Access to an [entity container](entity.md) representing the current character's _UI_ mouseover target if there is one, or their _field_ mouseover target otherwise. Note that the `O` in `Over` is capitalised.

- `HasMouseOverTarget`, readonly boolean|nil\
  Whether the current character _has_ a mouseover target, be it field or UI. This will be `nil` if you aren't logged in, but is _otherwise_ equivalent to `.MouseOverTarget.Exists`.

- `MapZone`, readonly number|nil (unsigned integer)\
  The user's current map zone, used to separate worldspaces. Will be zero if the current zone is indeterminate.

- `InCombat`, readonly boolean|nil\
  Whether or not the user is currently considered to be in combat by the game.

- `Mounted`, readonly boolean|nil\
  Whether or not the user is currently mounted. If `false`, the `MountId` property will be zero.

- `Flying`, readonly boolean|nil\
  Whether the user is currently flying on a mount. If you want to know whether the user is in the air but not _flying_ (jumping/falling), use `Jumping` instead.

- `Swimming`, readonly boolean|nil\
  Whether the user is currently in a swimming state.

- `Diving`, readonly boolean|nil\
  Whether the user is currently diving underwater.

- `Jumping`, readonly boolean|nil\
  Whether the user is currently jumping or falling in the air. To see if the user is _flying_, use the `Flying` property instead.

- `Crafting`, readonly boolean|nil\
  Whether the user is currently crafting.

- `Gathering`, readonly boolean|nil\
  Whether the user is currently gathering.

- `Fishing`, readonly boolean|nil\
  Whether the user is currently fishing.

- `Performing`, readonly boolean|nil\
  Whether the user is currently performing as a bard.

- `Casting`, readonly boolean|nil\
  Whether the user is currently casting something.

- `InCutscene`, readonly boolean|nil\
  Whether the user is currently in a cutscene.

- `Trading`, readonly boolean|nil\
  Whether the user is currently trading with another player.

- `InDuty`, readonly boolean|nil\
  Whether the user is currently in a duty instance.

- `UsingFashionAccessory`, readonly boolean|nil\
  Whether the user is currently using a fashion accessory, such as a parasol or wings.

- `WeaponDrawn`, readonly boolean|nil\
  Whether the user's weapon is currently drawn.

- `Moving`, readonly boolean|nil\
  Whether the game says the user is currently moving.

## Methods
The following methods are avilable on the `Player` API object.

- `boolean|nil HasEmote(string)`\
  Checks whether the current character has access to the given emote command. Leading slash (`/`) is optional, and _all_ game aliases are accepted, not just the main one. Do not pass a SimpleTweaks alias; they aren't real commands and will not be expanded.\
  Returns `true` if you are logged into a character that has access to the given emote, `false` if you are logged into a character that does _not_ have access, or `nil` if you aren't logged in or something breaks.

- `any|nil MF(any, any)`\
  If the current character is considered male (`.IsMale == true`), the first value will be returned. If it is considered female (`.IsFemale == true`), the second value will be returned. In all other cases (which should never occur), `nil` will be returned. This is a shortcut for `.Entity.MF()`.

- `any MFN(any, any, any)`\
  If the current character is considered male (`.IsMale == true`), the first value will be returned. If it is considered female (`.IsFemale == true`), the second value will be returned. In all other cases (which are not currently known to exist), the third value will be returned. This is a shortcut for `.Entity.MFN()`.

## Special
When converted to a string (such as by `tostring(Game.Player)`) you will get the current character's name and homeworld in the standard format, or an empty string if you aren't logged in.
