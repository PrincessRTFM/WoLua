# WoLua Entity Data
_Not the parahuman kind, don't worry._

## Usage
Entity containers are used to represent all interactable game objects. Anything that returns information about an object in the game, such as the player's targets, returns an entity container. A container representing the current character can be accessed via `Game.Player.Entity`.

Anything that returns an entity container will never return `nil`, but the returned container may represent a lack of an entity. See `Exists` below.

## Properties
The following properties exist on entity container objects. All of them are readonly.

- `Exists`, boolean\
  Indicates whether or not this container represents an entity that actually exists. If this is `false`, this container should not be used, as there is no game object behind it whatsoever.

- `Name`, string|nil\
  The textual name of this game object, if it exists. If there is no object behind this container, this will be `nil`.

- `Firstname`, string|nil\
  If this object represents a _player_, this will be their first name. If there is no object behind this container, this will be `nil`. In all other cases, this is equivalent to `.Name`.

- `Lastname`, string|nil\
  If this object represents a _player_, this will be their last name. If there is no object behind this container, this will be `nil`. In all other cases, this is equivalent to `.Name`.

- `IsMale`, boolean|nil\
  This will be true if the represented entity is considered male by the game. Note that minions are considered male even if the entity they represent (such as the Dress-up Y'shtola) is actually female. If this game object doesn't exist, this will be `nil`.

- `IsFemale`, boolean|nil\
  This will be true if the represented entity is considered female by the game. If this game object doesn't exist, this will be `nil`.

- `IsGendered`, boolean|nil\
  This will be true if the represented entity is considered to have a gender by the game. If this game object doesn't exist, this will be `nil`. In all other known cases, this _should_ be true. This is a equivalent for `.IsMale or .IsFemale`.

- `HasTitle`, boolean|nil\
  This will be `true` if the represented entity is a player who currently has a title, `false` if the represented entity is a player who does _not_ have a title, and `nil` if this object does not represent a (valid) player.

- `TitleText`, string|nil\
  If the represented entity is a player, this will either be their current title (accounting for gender where necessary) or an empty string if they don't have one. If the represented entity is not a (valid) player, this will be `nil`.\
  Note that some titles have different forms for male and female characters, such as "God of War" versus "Goddess of War". This property will automatically return the form that is appropriate for the represented player.

- `TitleIsPrefix`, boolean|nil\
  If the represented entity is a player, this will be `true` if they have a title that is a prefix (displays _above_ their name in their nameplate), `false` if their title is a suffix, or `nil` if they do not currently have a title. This will also be `nil` if the represented entity is not a (valid) player.

- `Type`, string|nil\
  The name of the type of this game object as known by Dalamud. If there is no object behind this container, this will be `nil`.

- `Alive`, boolean|nil\
  Whether this object is considered alive or not. May be nonsensical for certain objects, such as aetherytes. This will be `nil` if and only if there is no object behind this container.

- `Mount`, object\
  The [mount data](mount.md) for this entity. This will never be `nil`, but if this game object is not mounted for any reason, the mount data will represent a lack of mount.

- `HomeWorldId`, number|nil (unsigned short)\
  The internal (FFXIV) numeric ID of this player's _home_ world. May be useful in niche cases, but you probably want `.HomeWorld` instead. Note that if this container does not represent a _player_ then this will be `nil`.

- `CurrentWorldId`, number|nil (unsigned short)\
  The internal (FFXIV) numeric ID of this player's _current_ world. May be useful in niche cases, but you probably want `.CurrentWorld` instead. Note that if this container does not represent a _player_ then this will be `nil`.

- `HomeWorld`, string|nil\
  The textual name of this player's _home_ world. Note that if this container does not represent a _player_ then this will be `nil`.

- `CurrentWorld`, string|nil\
  The textual name of this player's _current_ world. Note that if this container does not represent a _player_ then this will be `nil`.

- `Target`, object\
  The entity container for this game object's target, if any. If there is no target, this container will exist but will represent nothing. See `.Exists`.

- `HasTarget`, boolean\
  Whether or not this game object has a target. This is a shortcut for `.Target.Exists`.

- `IsPlayer`, boolean\
  Indicates whether this container represents a valid game object that is a player. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsCombatNpc`, boolean\
  Indicates whether this container represents a valid game object that is a combat-capable NPC. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsTalkNpc`, boolean\
  Indicates whether this container represents a valid game object that is a non-combat NPC. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsNpc`, boolean\
  Indicates whether this container represents a valid game object that is either kind of NPC above. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsTreasure`, boolean\
  Indicates whether this container represents a valid game object that is some form of treasure. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsAetheryte`, boolean\
  Indicates whether this container represents a valid game object that is an aetheryte. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsGatheringNode`, boolean\
  Indicates whether this container represents a valid game object that is a gathering node. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsEventObject`, boolean\
  Indicates whether this container represents a valid game object that is an event object. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsMount`, boolean\
  Indicates whether this container represents a valid game object that is a mount. If this container does not represent a valid object, this will be `false`, _not_ `nil`. This is not likely to ever be used, as it is unlikely to be possible for a script to acquire an entity container for a mount.

- `IsMinion`, boolean\
  Indicates whether this container represents a valid game object that is a minion. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsRetainer`, boolean\
  Indicates whether this container represents a valid game object that is a retainer. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsArea`, boolean\
  Indicates whether this container represents a valid game object that is an area object. If this container does not represent a valid object, this will be `false`, _not_ `nil`. It is not currently known what "area" objects actually are.

- `IsHousingObject`, boolean\
  Indicates whether this container represents a valid game object that is a housing object. If this container does not represent a valid object, this will be `false`, _not_ `nil`.

- `IsCutsceneObject`, boolean\
  Indicates whether this container represents a valid game object that is a cutscene object. If this container does not represent a valid object, this will be `false`, _not_ `nil`. It is not currently known what "cutscene" objects actually are.

- `IsCardStand`, boolean\
  Indicates whether this container represents a valid game object that is a card stand. If this container does not represent a valid object, this will be `false`, _not_ `nil`. It is not currently known what "card stand" objects actually are.

- `IsOrnament`, boolean\
  Indicates whether this container represents a valid game object that is an ornament. If this container does not represent a valid object, this will be `false`, _not_ `nil`. It is not currently known what "ornament" objects actually are.

- `Level`, number|nil (unsigned byte)\
  The current level of this object, if it has one. If this object is not valid or does not have a level, this will be `nil`. If this is used on a player, it _will_ represent level sync effects.

- `Job`, object\
  The [job data](job.md) for this game object, if it has any. If not, the returned job data object will represent the inaccessible "adventurer" job with an ID of `0`.

- `CompanyTag`, string|nil\
  The FC tag for this character's FC, if any. If there is no tag, this will be an empty string. If this object cannot _have_ an FC tag, this will be `nil`.

- `Hp`, number|nil (unsigned integer)\
  The entity's _current_ HP, if applicable. If this object doesn't have HP (such as not being a living entity), this will be `nil`.

- `MaxHp`, number|nil (unsigned integer)\
  The entity's _maximum_ HP at the current level. If this object doesn't have HP (such as not being a living entity), this will be `nil`.

- `Mp`, number|nil (unsigned integer)\
  The entity's _current_ MP. If this object doesn't have MP (such as not being a living entity), this will be `nil`.

- `MaxMp`, number|nil (unsigned integer)\
  The entity's _maximum_ MP at the current level. If this object doesn't have MP (such as not being a living entity), this will be `nil`.

- `Gp`, number|nil (unsigned integer)\
  The entity's _current_ GP. If this object doesn't have GP (such as not being a living entity _or_ not being a gatherer), this will be `nil`.

- `MaxGp`, number|nil (unsigned integer)\
  The entity's _maximum_ GP at the current level. If this object doesn't have GP (such as not being a living entity _or_ not being a gatherer), this will be `nil`.

- `Cp`, number|nil (unsigned integer)\
  The entity's _current_ CP. If this object doesn't have CP (such as not being a living entity _or_ not being a crafter), this will be `nil`.

- `MaxCp`, number|nil (unsigned integer)\
  The entity's _maximum_ CP at the current level. If this object doesn't have CP (such as not being a living entity _or_ not being a crafter), this will be `nil`.

- `IsHostile`, boolean\
  Whether the game considers this entity to be hostile to the player. If it is not possible for this entity to ever be hostile, this will be `false`.

- `InCombat`, boolean\
  Whether this entity is currently engaged in combat. If it is not possible for this entity to engage in combat, this will be `false`.

- `WeaponDrawn`, boolean\
  Whether this entity has a weapon drawn. If this entity is not able to draw a weapon, this will be `false`.

- `IsPartyMember`, boolean\
  Whether this entity is a member of the current character's party. If this entity cannot join a party, this will be `false`.

- `IsAllianceMember`, boolean\
  Whether this entity is a member of the current character's alliance. If this entity cannot join a party, this will be `false`.

- `IsFriend`, boolean\
  Whether this player is on the current character's friends list. If this entity is not a player, this will be `false`.

- `IsCasting`, boolean\
  Whether this entity is currently "casting" an action, which may not be a spell. Using items with a use time also counts, for example. If this entity cannot use items, this will be `false`.

- `CanInterrupt`, boolean\
  Whether this entity's cast can be interrupted by the player. If `.IsCasting` is `false`, this will also be `false`.

- `PosX`, number|nil (floating point)\
  The internal (_not_ map coordinate) X position of this entity. This is one of the two horizontal coordinates. If this container does not represent a valid game object, this will be `nil`.

- `PosY`, number|nil (floating point)\
  The internal (_not_ map coordinate) Y position of this entity. This is the vertical coordinate. If this container does not represent a valid game object, this will be `nil`.

- `PosZ`, number|nil (floating point)\
  The internal (_not_ map coordinate) Z position of this entity. This is one of the two horizontal coordinates. If this container does not represent a valid game object, this will be `nil`.

- `RotationRadians`, number|nil (floating point)\
  The rotation of this game object in radians, ranging from `0` to `2*Pi`.

- `RotationDegrees`, number|nil (floating point)\
  The rotation of this game object in degrees, ranging from `0` to `360`.

- `FlatDistance`, number|nil (floating point)\
  The two-dimensional (horizontal) euclidean distance between this game object's position and the current character's position, ignoring hitbox radius. If this game object doesn't exist, this will be `nil`. This is a shortcut for calling `.FlatDistanceFrom(Game.Player)`.

- `Distance`, number|nil (floating point)\
  The three-dimensional euclidean distance between this game object's position and the current character's position, ignoring hitbox radius. If this game object doesn't exist, this will be `nil`. This is a shortcut for calling `.DistanceFrom(Game.Player)`.

## Methods
The following methods are avilable on the `Debug` API object.

- `number|nil FlatDistanceFrom(PlayerAPI|EntityContainer)`\
  Returns the two-dimensional (horizontal) euclidean distance between this game object and the one provided. If either game object is invalid, the return value will be `nil`. If you pass a [Player API](player.md) object, the current character will be implicitly used.

- `number|nil DistanceFrom(PlayerAPI|EntityContainer)`\
  Returns the three-dimensional euclidean distance between this game object and the one provided. If either game object is invalid, the return value will be `nil`. If you pass a [Player API](player.md) object, the current character will be implicitly used.

- `any|nil MF(any, any)`\
  If this game object is considered male (`IsMale == true`), the first value will be returned. If it is considered female (`IsFemale == true`), the second value will be returned. In all other cases (which are not currently known to exist), `nil` will be returned.

- `any MFN(any, any, any)`\
  If this game object is considered male (`IsMale == true`), the first value will be returned. If it is considered female (`IsFemale == true`), the second value will be returned. In all other cases (which are not currently known to exist), the third value will be returned.
