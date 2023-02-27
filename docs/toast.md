# WoLua Toast API
_Toast is bread held under direct heat until crisp. Wait, wrong kind of toast._

## Usage
The Toast API is a sub-component of the [Game API](game.md), which means it's accessible through `Game.Toast`. "Toast" popups are the messages that appear across your screen with a light background when you enter a new part of the map, or when you complete a quest objective, or when you get an error about not having a valid target.

## Methods
The following methods are avilable on the `Toast` API object.

- `nil Short(string)`
  Creates a toast popup with the "normal" look for around two seconds.

- `nil Long(string)`
  Creates a toast popup with the "normal" look for around four seconds.

- `nil Error(string)`
  Creates a toast popup with the "error" look.

- `nil TaskComplete(string)`
  Creates a toast popup with the "quest objective complete" look. Includes the checkmark appearing and the sound effect.

- `nil TaskComplete(string, boolean)`
  Creates a toast popup with the "quest objective complete" look. Includes the checkmark appearing. The boolean indicates whether to suppress the sound effect.

- `nil TaskComplete(string, unsigned integer)`
  Creates a toast popup with the "quest objective complete" look. Uses the game icon represented by the number in the second parameter, or `0` to not use any icon. If an icon is used, the standard sound effect will play.
  Icon IDs are well beyond the scope of this document.

- `nil TaskComplete(string, unsigned integer, boolean)`
  Creates a toast popup with the "quest objective complete" look. Uses the game icon represented by the number in the second parameter, or `0` to not use any icon. The boolean indicates whether to suppress the sound effect. If no icon is used, the sound will not play regardless.
  Icon IDs are well beyond the scope of this document.
