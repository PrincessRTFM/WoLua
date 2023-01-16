# WoLua persistent storage
_Do you remember what I told you yesterday?_

## Usage
Persistent storage isn't _technically_ an API, at least not directly. There are functions to manipulate the storage table [on the top-level `Script` API](script.md), but the actual storage table itself is just an ordinary table accessed through `Script.Storage` - albeit one you cannot directly overwrite - and can be used as such. In order to replace the storage table entirely, you must use the `Script.SetStorage(table)` method. This is to ensure that the storage table is _always_ a table.

This storage table is _fully_ persistent, with the contents being saved to disk whenever the script calls `Script.SaveStorage()`. Whenever a script is loaded, its storage table is loaded from the disk file (if it exists) _before_ the script runs, so while you can manually refresh the table from disk via `Script.ReloadStorage()` at any time, you don't need to do so during initialisation.

If you don't flush your script's storage table to disk, it continues to be semi-persistent, remaining between _script_ reloads. If WoLua itself unloads (eg, relaunching the game or updating the plugin) then all in-memory changes will be lost. This can actually be useful if you want a script to only remember things during a single game run, as long as you the plugin isn't updated while playing.

Please note that while you can _technically_ store any lua value in the storage table, it is actually stored _on disk_ as JSON, which means that things like functions will not save and load properly. Doing so will never be supported, given how complicated any attempt to do so would be, assuming it can even be done in the first place.
