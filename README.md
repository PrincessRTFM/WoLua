# WoLua
_Warrior of... Lua?_

![GitHub build status](https://img.shields.io/github/actions/workflow/status/PrincessRTFM/WoLua/build.yml?logo=github)
![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/PrincessRTFM/WoLua?label=version&color=informational)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/PrincessRTFM/WoLua/master?label=updated)
[![GitHub issues](https://img.shields.io/github/issues-raw/PrincessRTFM/WoLua?label=known%20issues)](https://github.com/PrincessRTFM/WoLua/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc)

## About
[![License](https://img.shields.io/github/license/PrincessRTFM/WoLua?logo=github&color=informational&cacheSeconds=86400)](https://github.com/PrincessRTFM/WoLua/blob/master/LICENSE)

WoLua is a plugin that allows users (like you!) to write custom, context-aware chat commands in [lua](https://www.lua.org/). These commands can read information from the game through WoLua's API, and are able to do things like printing messages to your local chatlog, send chat (commands _and_ plain text) to the server, and more.

## Installation
Type `/xlplugins` in-game to access the plugin installer and updater. Note that you will need to add [my custom plugin repository](https://github.com/PrincessRTFM/MyDalamudPlugins) (full instructions included at that link) in order to find this plugin.

## In-game usage
- Type `/wolua` to pull up a GUI with basic instructions and the configuration options.
- Type `/wolua reload` to rescan all command scripts while playing, in case you add, edit, or remove anything.
- Type `/wolua list` to list all loaded command scripts.
- Type `/wolua call <script> [<optional arguments...>]` to actually run a command script. Any additional text will be passed to the script for its own use.

## Writing scripts
Since WoLua exposes a significant amount of game information (not to mention the interface for _doing_ things) to scripts, the documentation is too extensive for the basic readme page. You can find details on the [dedicated API docs](https://github.com/PrincessRTFM/WoLua/tree/master/docs) instead.
