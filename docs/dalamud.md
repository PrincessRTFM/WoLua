# WoLua Dalamud API
_Your API to the API that hosts my API... hang on, I'm getting mixed up._

## Usage
The Dalamud API is a sub-component of the [Game API](game.md), which means it's accessible through `Game.Dalamud`.

## Properties
The following properties exist on the `Dalamud` API object.

- `Version`, readonly string|nil\
  The version of Dalamud that's currently loaded into the game. This is in the same format as plugin versions, `MAJOR.MINOR.BUILD.REVISION`.

## Methods
The following methods are avilable on the `Dalamud` API object.

- `boolean HasPlugin(string)`\
  Checks if a plugin with the provided **internal** name is both installed _and_ currently loaded. No version check is performed.\
  Please note that the internal name of a plugin is **not** the same as the name displayed in the plugin installer window, and it **is** case sensitive. You can use the `/wolua debug` window to see a list of all installed plugins (disabled ones will be greyed out) with their internal name, display name, and version.

- `boolean HasPlugin(string, string)`\
  Checks if a plugin with the provided **internal** name is both installed _and_ currently loaded, _and_ that the provided version is compatible with the loaded version. Plugin versions are in the form of `MAJOR.MINOR.BUILD.REVISION`. Version compatibility checks are performed as follows:
  - If the `MAJOR` components are different, the versions are _not_ compatible.
  - If the loaded `MINOR` component is _greater_ than the requested one, they _are_ compatible.
  - If the loaded `MINOR` component is _less_ than the requested one, they are _not_ compatible.
  - If the `MINOR` components are equal, the `BUILD` components are tested.
  - If the loaded `BUILD` component is _greater_ than the requested one, they _are_ compatible.
  - If the loaded `BUILD` component is _less_ than the requested one, they are _not_ compatible.
  - If the `BUILD` components are equal, the `REVISION` components are tested.
  - If the loaded `REVISION` component is _greater than **or** equal to_ the requested one, they _are_ compatible.
  - If the loaded `REVISION` component is _less_ than the requested one, they are _not_ compatible.
