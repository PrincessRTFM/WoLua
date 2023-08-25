# WoLua Game API
_How do you make a game script that can't touch the game?_

## Usage
The Game API is top-level, which means it's accessible through the global `Game`.

## Properties
The following properties exist on the `Game` API object.

- `Player`, API
  Access to the [Player API](player.md).

- `Chocobo`, API
  Access to the [Chocobo API](chocobo.md).

- `Toast`, API
  Access to the [Toast API](toast.md).

## Methods
The following methods are avilable on the `Game` API object.

- `nil PrintMessage(any...)`
  Prints the given values to the chatlog, concatenated by one space. Special values will be given more useful representations, such as tables rendering as JSON.

- `nil PrintError(any...)`
  Prints the given values to the chatlog in red, concatenated by one space. Special values will be given more useful representations, such as tables rendering as JSON.

- `nil SendChat(string)`
  Treats the given string as if the user had typed it into their chatlog input, except that it doesn't end up in their message history. **This is dangerous** because it can also send plain chat in whatever the user's current chat channel is! Do not accept untrusted input without carefully checking it over!
