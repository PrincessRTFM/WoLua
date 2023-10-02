# Printing Local Messages

So now you know how to create a basic script, and how to print debugging output. But what about actually _doing_ things? Well, step one is being able to print messages for the user of your script. This is important for offering help messages, and also printing errors if the user uses your script wrong - for instance, if your script needs some user input and they don't provide any.

There are two functions for this, both under the global [`Game` API](https://github.com/PrincessRTFM/WoLua/blob/master/docs/game.md): `PrintMessage(...)` and `PrintError(...)`. Both of these will join all provided arguments with a single space, and will attempt to turn non-string values into something more reasonable, just like `Script.Debug.Print(...)` does, as you learned in 02 (debugging). If debug output is enabled, they will also print a debug message to the log. In both cases, the message printed will be prefixed with `[WoLua][<script>]` for the sake of clarity to the player. The only actual difference is that `PrintMessage()` uses a light green colour for the message, while `PrintError()` uses an orange colour instead.

**Neither of these functions will send any data to the game server.**
