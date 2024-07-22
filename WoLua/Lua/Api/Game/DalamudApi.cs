using System;
using System.Linq;

using Dalamud.Plugin;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Lua.Docs;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
public class DalamudApi: ApiBase {
	public DalamudApi(ScriptContainer source) : base(source) { }

	[LuaDoc("The version hash for the currently loaded Dalamud framework. This is not a \"user-friendly\" version number, and you **CANNOT** compare it against others."
		+ " It exists mostly to provide debugging information, or to allow scripts to check for _specific_ versions that might break something.")]
	public static string Version { get; } = typeof(IDalamudPluginInterface).Assembly.GetName().Version?.ToString() ?? "UNKNOWN";

	#region Plugin check

	private IExposedPlugin? findPlugin(string name) {
		IExposedPlugin[] plugins = Service.Interface.InstalledPlugins.ToArray();
		this.Log($"Checking {plugins.Length} installed plugins for {name}");
		return plugins.FirstOrDefault(p => p.InternalName == name);
	}

	[LuaDoc("Check whether a given plugin is installed, using the **internal** name (not the user-friendly label) and optionally of at least the given minimum version."
		+ " Note that the version check borrows meaning from SemVer and will fail if the major version (the first number) is not the _same_, including if the loaded one is higher."
		+ " SemVer treats major version increments as **backwards-incompatible**, which would mean that a different value is potentially not compatible with whatever the script may wish to do."
		+ " However, plugin versions _do not_ use SemVer, so this may not always be correct. Talk to the plugin author if you have a problem (and tell them to use a sane versioning scheme).")]
	public bool HasPlugin(string pluginName, string? version = null) {
		IExposedPlugin? found = this.findPlugin(pluginName);

		if (found is null) {
			this.Log($"{pluginName} is not installed");
			return false;
		}
		if (!found.IsLoaded) {
			this.Log($"{found.Name} is installed but not loaded");
			return false;
		}

		Version? wanted = null;
		if (!string.IsNullOrEmpty(version)) {
			if (!System.Version.TryParse(version, out wanted))
				Service.Plugin.Error($"Invalid version [{version}] in HasPlugin() call, falling back to unversioned check");
		}
		if (wanted is not null) {
			if (found.Version.Major != wanted.Major || found.Version < wanted) {
				this.Log($"{found.Name} v{found.Version} is loaded, but not compatible with v{wanted}");
				return false;
			}
		}

		this.Log($"{found.Name} v{found.Version} is loaded{(wanted is not null ? $" and compatible with v{wanted}" : string.Empty)}");
		return true;
	}

	#endregion
}
