# WoLua Party API
_Just wait 'til I walk in..._

## Usage
The Party API is a sub-component of the [Player API](player.md), which means it's accessible through `Game.Player.Party`.

## Properties
The following properties exist on the `Party` API object.

- `Size`, readonly number|nil (technically signed integer)
  The size of the current character's current party.

- `InAlliance`, readonly boolean|nil
  If player data is available and valid (see `Loaded` in the [Player API](player.md)), represents whether the current character is in an alliance. Otherwise, this will be `nil`.

- `InParty`, readonly boolean|nil
  If player data is available and valid (see `Loaded` in the [Player API](player.md)), represents whether the current character is in a party. Otherwise, this will be `nil`.

## Special
When indexed as a table (`Game.Player.Party[index]`), returns an [entity container](entity.md) for the party member at that index. If there is no party member, a container will still be returned, representing a lack of entity.

The size operator (`#Game.Player.Party`) will also return the size of the party.
