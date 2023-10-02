# Debugging Your Script

Nobody gets it right the first time. That means you're gonna need to debug your scripts somehow. For that, the [`Debug` API](https://github.com/PrincessRTFM/WoLua/blob/master/docs/debug.md) will be invaluable. Step one, of course, is to turn debugging output on with `Script.Debug.Enabled = true`, probably at the very top of your script.

**Debug output is only enabled for debug calls made while `Script.Debug.Enabled` is `true`.**

This sounds obvious, but it means that if you do anything _before_ turning that on, then nothing will show up. If you put that inside your registered callback function, for instance, then debugging output will _only_ be enabled once your script command is used; any setup or initialisation that you do will be run _without_ debug output.

As a side note, if you run a debug _build_ of WoLua - which you can only do by compiling it yourself in debug mode and loading it as a local/developer plugin - then _all_ scripts are run with debug mode _forcibly_ enabled, and it cannot be disabled. You can check this in scripts by looking at the readonly `Script.Debug.PluginDebugBuild` flag.

Anyway, now that you've enabled debugging, step two is to see the debug output: the dalamud plugin log. Use `/xllog` to open it up, and don't worry about all the stuff in there right now. You're going to want to look at lines starting with `[WoLua]`. Specifically, _your_ script will print debug information prefixed with `[WoLua] [<origin>:<your-script-name>|<category>]` where `<origin>` will _probably_ be `SCRIPT` for the stuff you care about. However, debugging output being enabled _does_ also turn on some system-level debugging messages, such as module resolution for `require()`.

And now that you've enabled debugging _and_ found how to actually _read_ the debug output, step three is, of course, to print debugging output to see. There are a few ways to do this:

- `Script.Debug.PrintString("your message here")` will print the provided _single string value_ to the debug log. It will be prefixed with `[WoLua] [SCRIPT:<script>|DEBUG]` to let you look for it more easily.
- `Script.Debug.Print(...)` will concatenate all given values with a single space, and any special values will be transformed into a hopefully more useful form, like tables being rendered as JSON. The prefix will be exactly the same.
- `Script.Debug.DumpStorage()` will print your script's _current_ persistent storage as a JSON table, prefixed with `[WoLua] [SCRIPT:<script>|STORAGE]`. This is a simple and easy way to see what the script sees. Note that this function _doesn't_ touch the storage file on disk, so you may wish to use `Script.SaveStorage()` or `Script.ReloadStorage()` along with it.
- `Script.Debug.Dump(...)` will print a header line (`[WoLua] [SCRIPT:<script>|DEBUG] BEGIN VALUE DUMP: <count>`), then print _each individual value provided_ on its own line as if passed to `.PrintString()` in the form `<number>: <value>`, followed by a footer line (`[WoLua] [SCRIPT:<script>|DEBUG] END VALUE DUMP`). Each non-string value is given a more useful representation just like `.Print()` above does. You could do this manually, or just use a series of `.Print()` calls, but this method is provided for convenience when trying to print a series of values all at once.

Additionally, you can call `Script.Debug(...)` as a function, which is exactly identical to calling `Script.Debug.Print(...)` as explained above. Finally, you can use the lua builtin `print("message")` function, which is handled by `Script.Debug.PrintString()` above.
