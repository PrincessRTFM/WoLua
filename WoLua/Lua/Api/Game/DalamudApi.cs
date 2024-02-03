using System;
using System.Linq;

using Dalamud.Plugin;

using MoonSharp.Interpreter;

namespace PrincessRTFM.WoLua.Lua.Api.Game;

[MoonSharpUserData]
public class DalamudApi: ApiBase { // TODO luadoc all of this
	public DalamudApi(ScriptContainer source) : base(source) { }

	public static string Version { get; } = typeof(DalamudPluginInterface).Assembly.GetName().Version?.ToString() ?? "UNKNOWN";

	#region Plugin check

	private InstalledPluginState? findPlugin(string name) {
		InstalledPluginState[] plugins = Service.Interface.InstalledPlugins.ToArray();
		this.Log($"Checking {plugins.Length} installed plugins for {name}");
		return plugins.FirstOrDefault(p => p.InternalName == name);
	}

	public bool HasPlugin(string pluginName, string? version = null) {
		InstalledPluginState? found = this.findPlugin(pluginName);

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
