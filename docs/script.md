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

- `PluginCommand`, readonly string
  The WoLua base command, including the leading slash (`/wolua`, unless it changes in the future for some reason) for convenience in case of forks or changes.

- `Name`, readonly string
  The invocation name of the script, valid for passing to `/wolua call`.

- `CallSelfCommand`, readonly string
  A shortcut to join `Script.PluginCommand`, "call", and `Script.Name` with a single space. This provides the entire command necessary to invoke the current script.

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

### Special
When invoked as a function (`Script()`), requires one argument which must be a function, and registers that function as the script's callback when it's used via `/wolua call`. The registered function _may_ take one argument, which will be a string: if any additional text is provided to `/wolua call`, that text will be passed to the callback function; otherwise, the function will receive an empty string.
