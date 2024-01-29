# WoLua FATE Data
_Are you a fortune teller?_

## Usage
FATE containers are used to represent all game FATEs. They provide details as well as helpful shortcut calculations, all with enforced safety.

Anything that returns a FATE container will never return `nil`, but the returned container may represent a lack of a FATE. See `Valid` and `Exists` below.

## Properties
The following properties exist on FATE container objects.

### Validation

- `Valid`, readonly boolean\
  Indicates whether or not this container represents a legitimate FATE in the game's memory. If this is `false`, this container should not be used.

- `Exists`, readonly boolean\
  Indicates whether or not this container represents a FATE that exists in the _world_. FATEs that are waiting to be triggered by a player, currently in progress, or waiting to end (such as item turn-ins) all exist in the world. FATEs that have ended do not, even if their data may still exist in the game's memory.

### Display

- `Name`, readonly string|nil\
  The textual name of this FATE.\
  If this FATE is not _valid_, this will be `nil`.

- `Progress`, readonly number|nil (unsigned byte)\
  The progress towards this FATE's completion, as a percentage.\
  If this FATE does not _exist_, this will be `nil`.

### Levels

- `MinLevel`, readonly number|nil (unsigned byte)\
  The minimum level of this FATE. You are recommended to be at least this level to participate. If you are below this level, the game weighs your contribution less, and combat is expected to be much harder.\
  If this FATE is not _valid_, this will be `nil`.

- `MaxLevel`, readonly number|nil (unsigned byte)\
  The maximum level of this FATE, above which you need to manually level sync down in order to participate.\
  If this FATE is not _valid_, this will be `nil`.

### Time

- `Duration`, \
  The total duration of this FATE, in seconds.\
  If this FATE is not _valid_, this will be `nil`.

- `TimeLeft`, \
  The remaining time available to complete this FATE, in seconds.\
  If this FATE is not _valid_, this will be `nil`. If this FATE has not yet started, this will be equal to `.Duration`. If this FATE has already ended, this will be `0`.

- `TimeElapsed`, \
  The amount of time this FATE has been running for so far.\
  If this FATE is not _valid_, this will be `nil`. If this FATE has not yet started, this will be `0`. If this FATE has already ended, this will be equal to `.Duration`.

### States

- `Waiting`, readonly boolean\
  Whether this FATE is in the preparation phase, before it is actually started.\
  If this FATE does not _exist_, this will be `false`, _not_ `nil`.

- `Running`, readonly boolean\
  Whether this FATE is currently in progress.\
  If this FATE does not _exist_, this will be `false`, _not_ `nil`.

- `Ending`, readonly boolean\
  Whether this FATE is waiting to end, as with item turnin FATEs once enough items have been handed in.\
  If this FATE does not _exist_, this will be `false`, _not_ `nil`.

- `Active`, readonly boolean\
  Whether this FATE is considered "active", either waiting to be triggered or currently running. This does _not_ include waiting to end.\
  This property is provided for convenience, to filter out FATEs that can actually be participated in.\
  If this FATE does not _exist_, this will be `false`, _not_ `nil`.

### Position

- `PosX`, readonly number|nil (floating point)\
  The internal (_not_ map coordinate) X position of this FATE. This is one of the two horizontal coordinates.\
  If this FATE does not _exist_, this will be `nil`.

- `PosY`, readonly number|nil (floating point)\
  The internal (_not_ map coordinate) Y position of this FATE. This is one of the two horizontal coordinates.\
  Note that this is technically the _Z_ coordinate, since the game engine uses X/Z for horizontal position and Y for vertical. For the sake of consistency with the map coordinates displayed to the player, WoLua swaps them.\
  If this FATE does not _exist_, this will be `nil`.

- `PosZ`, readonly number|nil (floating point)\
  The internal (_not_ map coordinate) Z position of this FATE. This is the vertical coordinate.\
  Note that this is technically the _Y_ coordinate, since the game engine uses X/Z for horizontal position and Y for vertical. For the sake of consistency with the map coordinates displayed to the player, WoLua swaps them.\
  If this FATE does not _exist_, this will be `nil`.

- `MapX`, readonly number|nil (floating point)\
  The player-friendly map-style X (east/west) coordinate of this FATE.\
  If this FATE does not _exist_, this will be `nil`.

- `MapY`, readonly number|nil (floating point)\
  The player-friendly map-style Y (north/south) coordinate of this FATE.\
  If this FATE does not _exist_, this will be `nil`.

- `MapZ`, readonly number|nil (floating point)\
  The player-friendly map-style Z (height) coordinate of this FATE.\
  If this FATE does not _exist_, this will be `nil`.

- `MapCoords`, 3x readonly number|nil (floating point)\
  This property returns _three_ numbers, corresponding (respectively) to `.MapX`, `.MapY`, and `.MapZ`, for convenience and efficiency when more than one coordinate is desired.\
  Since WoLua must calculate the coordinates fresh for each of those calls, it is recommended to use this property instead when you want multiple coordinates.\
  If this FATE does not _exist_, this will be `nil`.

- `Radius`, readonly number|nil (floating point)\
  This is the (planar) radius of this FATE. The game defines FATEs as cylinders, with their position being the central point at the bottom. This is the horizontal distance from the central line running the entire height of the FATE that you can go before you leave the FATE.\
  It is not currently possible to determine the height of a FATE.\
  If this FATE is not _valid_, this will be `nil`.

### Distance

- `FlatDistanceToCenter`/`FlatDistanceToCentre`, readonly number|nil (floating point)\
  The two-dimensional (horizontal) euclidean distance between the player and this FATE.\
  The game defines FATEs as cylinders, with their position being the central point at the bottom.\
  If this FATE does not _exist_, this will be `nil`.

- `DistanceToCenter`/`DistanceToCentre`, readonly number|nil (floating point)\
  The three-dimensional euclidean distance between the player and this FATE.\
  The game defines FATEs as cylinders, with their position being the central point at the bottom.\
  If this FATE does not _exist_, this will be `nil`.

- `FlatDistanceToEdge`, readonly number|nil (floating point)\
  The two-dimensional (horizontal only) distance between the player and the edge of this FATE.\
  The game defines FATEs as cylinders, with their position being the central point at the bottom. This property does _not_ account for the FATE's radius _or_ height.\
  If this FATE does not _exist_, this will be `nil`.

> :warning: There is method to calculate the three-dimentional distance to the edge of a FATE, due to the inability to get the height of the FATE's bounding cylinder. Attempting to calculate the distance to the cenre minus the radius only provides the distance to a _spherical_ edge around the FATE's position, which is not representative of the actual bounds.

## Special
Converting a FATE wrapper to a string provides the name, state, minimum level, and maximum level, provided the FATE is _valid_. If it is not, the result is instead `FATE[invalid]`.
