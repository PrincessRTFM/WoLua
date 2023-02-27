# WoLua Keys API
_A key part of multi-purpose scripts._

## Usage
The Keys API is a sub-component of the [Script API](script.md), which means it's accessible through `Script.Keys`.

### Properties
The following properties exist on the `Keys` API object.

- `Control`, readonly boolean
  Whether or not the _control_ modifier key is down.

- `Ctrl`, readonly boolean
  Another name for the `Control` property.

- `Alt`, readonly boolean
  Whether or not the _alt_ modifier key is down.

- `Shift`, readonly boolean
  Whether or not the _shift_ modifier key is down.

## Future plans
This may be expanded to include other keys at some point, if a reason for such is ever found. However, given that other keys are all passed through to the game and WoLua's scripts are used as chat commands, no such reason currently exists.
