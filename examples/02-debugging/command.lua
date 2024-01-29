-- remember, if the PLUGIN is a debug build, then ALL scripts are run with debug mode enabled, and it CANNOT be turned off
-- it's assumed that if you've gone to the trouble of compiling and loading a debug build, you want to debug things
if Script.Debug.PluginDebugBuild then
	print("This WILL print to the debug log, because debug builds of the plugin force-enable debug output.")
else
	print("This will not show up in the debug log, because debug output isn't enabled yet.")
end

-- usually, when you want to debug a script, this will be the very top line
-- but you can also embed this in your script's callback function to allow contextual logic to control debug messages
-- just remember that this setting RESETS on script reloads, but does NOT reset between command calls!
Script.Debug.Enabled = true
print("This message prints during script initialisation, when WoLua is scanning and loading scripts.")

-- as mentioned in 01 (absolute minimum), you can use an anonymous function here and it'll work just fine
-- all that matters is that you pass a function
Script(function()
	Script.Debug.PrintString("This message shows up whenever you use this script's command.")
	Script.Debug.Print(
		"Calling `print()` and calling `Script.Debug.PrintString()` are precisely equivalent, because they use the same internal function.",
		"However, calling `Script.Debug.Print()` works differently.",
		"Each of these three separate strings will be joined together by a single space, and the result will be printed to the debug log."
	)
	Script.Debug(
		"Calling `Script.Debug()` as a function is precisely equivalent to `Script.Debug.Print()`",
		"because they also use the same internal function."
	)
	print("This script has no persistent storage data unless you manually create a file for it,"
		.. " but in order to demonstrate the `DumpStorage()` functionality, we're going to throw a few things together.")
	Script.Debug.DumpStorage()
	print("As you can see, that was (unless you messed with the disk files) an entirely empty storage table.")
	Script.Storage.StringValue = "some arbitrary text"
	Script.Storage.TableValue = { 1, 2, 3, 4, "five" }
	Script.Storage.BooleanValue = false
	Script.Debug.DumpStorage()
	print(
	"None of that is saved to disk right now - it's transient persistent storage, remaining between SCRIPT reloads but not between PLUGIN reloads.")
	print("But it showed up as the storage dump, because that's the CURRENT value of the IN-MEMORY storage table.")
	Script.Debug.Dump("a string", 23, false, { 1, "apple" }, Script.Storage, Script.Debug)
	print("As you can see, calling `Dump()` looks very different.")
	print(
	"Also, \"userdata\" values - which are basically magic lua values that point at plugin-internal C# things - are given special labels to make it clear that they aren't ordinary lua tables.")
end)
