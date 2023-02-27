# WoLua Mount Data
_Thankfully not the kind of mount you find in Limsa_

## Usage
Mount data is exposed via the `Mount` property of [entity containers](entity.md) for valid game objects, and also as a shortcut on the `Mount` property of the [Player API](player.md).

## Properties
The following properties exist on all mount data objects. All of them are readonly.

- `Id`, number (unsigned short)
  The internal (FFXIV) numeric ID of this mount. This value is unique for every different mount in the game. This value will be `0` if and only if `Active` is `false`.

- `Active`, boolean
  Whether this mount data object represents a mount or a lack of one. If this is `false` then the [entity container](entity.md) from which this mount data object was accessed is not currently using a mount.

- `Name`, string|nil
  The (lowercase, singular) name of the mount represented by this object, or `nil` if this object does not represent a mount. This will be `nil` if `Active` is `false`.

- `LowercaseArticle`, string|nil
  The lowercase form of either "a" or "an" for this mount, depending on whether it starts with a vowel. If `Active` is `false`, this will be `nil`.

- `UppercaseArticle`, string|nil
  The uppercase form of either "A" or "An" for this mount, depending on whether it starts with a vowel. If `Active` is `false`, this will be `nil`.
