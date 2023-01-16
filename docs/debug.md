# WoLua Debug API
_Because nothing is ever bug-free._

## Usage
The Debug API is a sub-component of the Script API, which means it's accessible through `Script.Debug`.

Please note that _all_ debugging methods print their output to Dalamud's debug log, accessible through `/xllog`, and NOT to the user's chatlog. The debug log contains debugging messages from _all_ loaded plugins, so you may wish to clear the log before debugging a script.

Further note that all debugging methods will _only_ produce output if you explicitly turn debugging on, which is a per-script setting. Otherwise, all debugging methods are effectively no-ops.

## Properties
The following properties exist on the `Debug` API object.

- `Enabled`, writable boolean
  Controls all output from the Debug API. Defaults to false; if you're debugging your script, you probably want it enabled at the top with `Script.Debug.Enabled = true`.

## Methods
The following methods are avilable on the `Debug` API object.

- `nil PrintString(string)`
  Prints the given string as-is to the debug log.

- `nil Print(any...)`
  Prints the given values to the debug log, concatenated by one space. Special values will be given more useful representations, such as tables rendering as JSON.

- `nil DumpStorage()`
  Prints the script's _current_ persistent storage as a JSON table. Does not touch the stored disk file, if it exists.

- `nil Dump(any...)`
  Prints all of the given values on individual lines with indices. Special values will be given more useful representations, such as tables rendering as JSON.

## Special
As a shortcut, when invoked as a function (`Script.Debug()`) all provided arguments are passed to the `Print()` method listed above. The standard lua `print()` function is implemented via the `PrintString()` method listed above.
