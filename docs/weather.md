# WoLua Weather Data
_Raindrops keep falling on my head, they keep falling on my head..._

## Usage
Weather data is exposed via the `Weather` property of the [Game API](game.md).

## Properties
The following properties exist on all weather data objects. All of them are readonly.

- `Id`, number (unsigned integer)\
  The raw, internal ID for this type of weather. Useful for checking what the weather is at the moment, without worrying about languages.

- `Name`, string\
  The user-friendly name of this type of weather. This is the same as the value shown when you mouseover the weather icon on your minimap.

- `Description`, string\
  A small description the game provides for more lore/RP friendly display of how the weather looks. Note that some special weather may sound strange.

## Special
When converted to a string (such as by `tostring(Game.Weather)`) you will get the current weather's name in lowercase, instead of the Title Case offered by the `.Name` property.
