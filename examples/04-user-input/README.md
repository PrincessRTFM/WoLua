# User Input

WoLua scripts can access game data for context, but it's also important to be able to take user input. That's how you can create commands like `/em does a thing`, where `does a thing` is used by the command in some way. Luckily, this is incredibly simple: your script's callback function is provided a string containing the rest of the text the user provided when they ran it. `/wolua call myscript additional text` will pass `additional text` to `myscript`'s callback function, and then the script can do whatever it wants with that.

Note that your script function doesn't _need_ to accept any arguments. If you don't actually care about user input, you can define the function to not take any arguments and it will continue to work just fine.

**IMPORTANT NOTE** - WoLua only strips _one_ space between the script name and provided arguments. If you use `/wolua call myscript    additional text` (with _four_ spaces between `myscript` and `additional text`) then the script will receive `   additional text` (with _three_ leading spaces) as an argument. This is done intentionally to allow things like ASCII-art text lines to be provided without getting mangled. You can strip those leading spaces in your script if you need to.
