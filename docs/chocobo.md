# WoLua Chocobo API
_Kweh? Kweh!_

## Usage
The Chocobo API is a sub-component of the [Game API](game.md), which means it's accessible through `Game.Chocobo`.

## Properties
The following properties exist on the `Chocobo` API object.

### Stats

- `Name`, readonly string|nil
  Your chocobo's name. This will be `nil` if you aren't logged in.

- `Unlocked`, readonly boolean|nil
  Whether or not you have unlocked your combat chocobo. **This is _not_ about unlocking your chocobo _mount_.** This will be `nil` if you aren't logged in.

- `CurrentHp`, readonly number|nil (unsigned integer)
  Your chocobo's current health, if it's present. This will be `nil` if you aren't logged in, _or_ if your chocobo is not currently _present_.

- `MaxHp`, readonly number|nil (unsigned integer)
  Your chocobo's maximum health, if it's present. This will be `nil` if you aren't logged in, _or_ if your chocobo is not currently _present_.

### Presence

- `TimeLeft`, readonly number|nil (float)
  How much time is left on your summoned chocobo, in seconds. If your chocobo is summoned but you're mounted, this value will be frozen, as your chocobo summon timer doesn't tick while you're mounted up. This will be `nil` if you aren't logged in.

- `Summoned`, readonly boolean|nil
  Whether or not your chocobo is currently summoned. This does _not_ indicate whether it is currently _present_, as it counts as summoned even when you're mounted. This check is equivalent to `.TimeLeft > 0`. This will be `nil` if you aren't logged in.

### Levelling

- `CurrentXP`, readonly number|nil (unsigned integer)
  Your chocobo's current experience towards the next level. This is _not_ your chocobo's _total_ experience. This will be `nil` if you aren't logged in.

- `Rank`, readonly number|nil (unsigned byte)
  Your chocobo's current rank, or _overall_ level. This will be `nil` if you aren't logged in.

- `Stars`, readonly number|nil (unsigned byte)
  The number of stars your chocobo has. This will be `nil` if you aren't logged in.

- `SkillPoints`, readonly number|nil (unsigned byte)
  The number of _unspent_ skill points your chocobo has. This will be `nil` if you aren't logged in.

- `AttackerLevel`, readonly number|nil (unsigned byte)
  The number of levels in the "attacker" (DPS) tree your chocobo has. Each skill unlock in the tree is another level. This will be `nil` if you aren't logged in.

- `DefenderLevel`, readonly number|nil (unsigned byte)
  The number of levels in the "defender" (tank) tree your chocobo has. Each skill unlock in the tree is another level. This will be `nil` if you aren't logged in.

- `HealerLevel`, readonly number|nil (unsigned byte)
  The number of levels in the "healer" tree your chocobo has. Each skill unlock in the tree is another level. This will be `nil` if you aren't logged in.

## Special
When converted to a string (such as by `tostring(Game.Chocobo)`) you will get your chocobo's name, or an empty string if you aren't logged in.
