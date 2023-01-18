# WoLua Script API
_Despite the name, it's only one part of the scripting API._

## Usage
The Script API is top-level, which means it's accessible through the global `Script`.

### Properties
The following properties exist on the `Script` API object.

- `Storage`, readonly table
  Access to the [persistent storage table](storage.md).

- `Debug`, API
  Access to the [Debug API](debug.md).

- `Keys`, API
  Access to the [Keys API](keys.md).

- `PluginCommand`, readonly string
  The WoLua base command, including the leading slash (`/wolua`, unless it changes in the future for some reason) for convenience in case of forks or changes.

- `Name`, readonly string
  The invocation name of the script, valid for passing to `/wolua call`. This may or may not be the same as `Title`, as spaces are stripped out from the folder name on disk.

- `Title`, readonly string
  The original name of the script. This may or may not be the same as `Name`; do _not_ attempt to pass it to `/wolua call` as it may have spaces in it. This value is for display only, and is the name of the folder on disk.

- `CallSelfCommand`, readonly string
  A shortcut to join `Script.PluginCommand`, "call", and `Script.Name` with a single space. This provides the entire command necessary to invoke the current script.

- `QueueSize`, readonly number (integer)
  The number of actions currently sitting in this script's action queue, waiting to be executed.

- `Clipboard`, writable string
  The current text on the system clipboard. Can be set as well, in order to set the clipboard text.

### Methods
The following methods are avilable on the `Script` API object.

- `boolean SaveStorage()`
  Saves the script's persistent storage to disk. This is NEVER done automatically, and MUST be done to save storage between plugin loads, although _script_ reloads do not reset unsaved persistent storage.
  Returns `true` if storage was successfully written to disk, `false` otherwise.

- `boolean ReloadStorage()`
  Reloads the script's persistent storage from disk, if available. This is done automatically when the script is initially loaded, _before+ it executes, so you don't need to do this manually unless you want to reset unsaved changes.
  Returns `true` if storage was successfully loaded, `false` if no storage file existed on disk for this script, or `nil` if an error occurred.

- `boolean DeleteStorage()`
  Clears the existing persistent storage _and_ delete the disk file if it exists. **This _cannot_ be undone!**
  Returns `true` on success, `false` if an error occurred.

- `nil SetStorage(table)`
  Replaces the **in-memory** persistent storage with the table provided; nothing on disk is touched, so you have to save manually if you want to keep the changes.

- `nil ClearQueue()`
  Immediately empties this script's action queue. This can be useful if you want to restart from the beginning when the script command is rerun, like how vanilla macros work. Please note that, due to technical reasons, if the queue is non-empty and your script queues more things, the existing queue will _not_ be replaced, nor will a new queue be created to run in parallel.

- `nil QueueDelay(unsigned integer)`
  Adds a delay to the script's action queue for the given number of _milliseconds_. Any higher precision is meaningless given network latency.

- `nil QueueAction(function, any...)`
  Adds the given function to the script's action queue, to be called with the provided arguments. WoLua API calls can be queued in this manner, allowing you to space out messages printed to the local chatlog or sent to the server, or to create [toast messages](toast.md) after a pause.

- `table|nil ParseJson(string)`
  Parses the provided string as JSON into a table. On success, returns the deserialised table. On failure, returns nil.
  Note that MoonSharp (the lua engine used by WoLua) also provides a `json` lua API by itself, with `json.parse(string)` and `json.serialize(table)` methods; however, if the provided JSON string for `json.parse()` is invalid, a _lua_ error will be thrown. If you don't catch it using `pcall`/`xpcall`, your entire script will error and be disabled.
  :warning: **You cannot deserialise anything other than JSON objects and arrays.** The MoonSharp API itself does not support anything but lua tables, which are represented as objects or arrays in JSON syntax depending on their keys.

- `string SerialiseJson(table)` (also named `SerializeJson`)
  Serialises the given table to a JSON string and returns it.
  Note that MoonSharp (the lua engine used by WoLua) also provides a `json` lua API by itself, with `json.parse(string)` and `json.serialize(table)` methods; however, if the provided value for `json.serialize()` is invalid, a _lua_ error will be thrown. If you don't catch it using `pcall`/`xpcall`, your entire script will error and be disabled.
:warning: **You cannot serialise non-table values.** The MoonSharp API itself does not support anything but lua tables, which are represented as objects or arrays in JSON syntax depending on their keys.

### Special
When invoked as a function (`Script()`), requires one argument which must be a function, and registers that function as the script's callback when it's used via `/wolua call`. The registered function _may_ take one argument, which will be a string: if any additional text is provided to `/wolua call`, that text will be passed to the callback function; otherwise, the function will receive an empty string.
