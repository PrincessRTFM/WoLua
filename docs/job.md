# WoLua Job Data
_Toast is bread held under direct heat until crisp. Wait, wrong kind of toast._

## Usage
Job data is exposed via the `Job` property of the `Player` API, which means it's accessible through `Game.Player.Job`.

## Properties
The following properties exist on job data objects. All of them are readonly. If you access job data while no character data is loaded, the `Id` will be zero and the various name properties will be `nil`.

- `Id`, number (unsigned integer)
  The internal (FFXIV) numeric ID of the current class or job. Unique for each; jobs do not share IDs with their classes.

- `Name`, string|nil
  The long-form (full) name of the represented class or job.

- `Abbreviation`, string|nil
- `Abbr`
- `ShortName`
  The short-form (three-letter) name of the represented class or job.

- `IsCrafter`, boolean
  Whether or not this class is a Disciple of the Hand.

- `IsGatherer`, boolean
  Whether or not this class is a Disciple of the Land.

- `IsMeleeDPS`, boolean
  Whether or not this class is a melee DPS role.

- `IsRangedDPS`, boolean
  Whether or not this class is a ranged DPS role.

- `IsMagicDPS`, boolean
  Whether or not this class is a magic DPS role.

- `IsHealer`, boolean
  Whether or not this class is a healer role.

- `IsTank`, boolean
  Whether or not this class is a tank role.

- `IsDPS`, boolean
  Whether or not this class is _any_ DPS role.

- `IsDiscipleOfWar`, boolean
  Whether or not this class is a Disciple of War (physical damage).

- `IsDiscipleOfMagic`, boolean
  Whether or not this class is a Disciple of Magic (magic damage).

- `IsBlu`, boolean
  Whether or not this specific job is Blue Mage.

- `IsLimited`, boolean
  Whether or not this class or job is limited. Currently identical to `IsBlu`, but may change with future game updates.
