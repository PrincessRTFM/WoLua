# The Absolute Minimum

The _absolute barest minimum_ that a WoLua script needs is to register a callback function. It doesn't actually even need to _do_ anything, but if you fail to register a function, then your script is considered broken, and attempting to call it will print an error in your chatlog. Additionally, when the script is first loaded, an error will be printed about it not registering a function.

This is accomplished by [calling the global `Script` object as a function and passing it a single function argument](https://github.com/VariableVixen/WoLua/blob/master/docs/README.md#script-callback-functions), as demonstrated in [`command.lua`](command.lua) here. Note that since this is an _empty_ function, nothing will happen when you call the script command.
