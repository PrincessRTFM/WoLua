# WoLua Job Data
_Work work work..._

## Usage
Job data is exposed via the `Job` property of an [entity container](entity.md), which means it's accessible through `Game.Player.Entity.Job` for the current character.

## Properties
The following properties exist on job data objects. All of them are readonly.

### Validation

- `Id`, number (unsigned integer)\
  The internal (FFXIV) numeric ID of the current class or job. Unique for each; jobs do not share IDs with their classes. If you access job data for an invalid entity (such as the current character while it isn't loaded), this will be `0`.

- `Valid`, boolean\
  Indicates whether this object represents a valid job. If not, then `.Id` will be `0`, `.Name` will be `adventurer`, and `.Abbreviation` (and aliases) will be `ADV`.

### Display

- `Name`, string|nil\
  The long-form (full) name of the represented class or job. This value is always in all lowercase. If you access job data for an invalid entity (such as the current character while it isn't loaded), this will be `adventurer`.

- `Abbreviation`, `Abbr`, `ShortName`, string|nil\
  The short-form (three-letter) name of the represented class or job. This value is always in all uppercase. If you access job data for an invalid entity (such as the current character while it isn't loaded), this will be `ADV`.

### Roles

- `IsCrafter`, boolean\
  Whether or not this class is a Disciple of the Hand.

- `IsGatherer`, boolean\
  Whether or not this class is a Disciple of the Land.

- `IsMeleeDPS`, boolean\
  Whether or not this class is a melee DPS role.

- `IsRangedDPS`, boolean\
  Whether or not this class is a ranged DPS role.

- `IsMagicDPS`, boolean\
  Whether or not this class is a magic DPS role.

- `IsHealer`, boolean\
  Whether or not this class is a healer role.

- `IsTank`, boolean\
  Whether or not this class is a tank role.

- `IsDPS`, boolean\
  Whether or not this class is _any_ DPS role.

- `IsDiscipleOfWar`, boolean\
  Whether or not this class is a Disciple of War (physical damage).

- `IsDiscipleOfMagic`, boolean\
  Whether or not this class is a Disciple of Magic (magic damage).

- `IsBlu`, boolean\
  Whether or not this specific job is Blue Mage.

- `IsLimited`, boolean\
  Whether or not this class or job is limited. Currently identical to `.IsBlu`, but may change with future game updates.
