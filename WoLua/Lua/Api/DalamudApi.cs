namespace PrincessRTFM.WoLua.Lua.Api;

using System;
using System.Linq;

using Dalamud.Plugin;

using MoonSharp.Interpreter;

[MoonSharpUserData]
public class DalamudApi: ApiBase {
	public DalamudApi(ScriptContainer source) : base(source) {
	}

	public static string Version { get; } = typeof(DalamudPluginInterface).Assembly.GetName().Version?.ToString() ?? "UNKNOWN";

	#region Plugin check

	private InstalledPluginState? findPlugin(string name) {
		InstalledPluginState[] plugins = Service.Interface.InstalledPlugins.ToArray();
		this.Log($"Checking {plugins.Length} installed plugins for {name}");
		return plugins.FirstOrDefault(p => p.InternalName == name);
	}

	public bool HasPlugin(string pluginName) {
		InstalledPluginState? found = this.findPlugin(pluginName);
		if (found is null) {
			this.Log($"{pluginName} is not installed");
			return false;
		}
		if (!found.IsLoaded) {
			this.Log($"{found.Name} is installed but not loaded");
			return false;
		}
		this.Log($"{found.Name} v{found.Version} is loaded");
		return true;
	}
	public bool HasPlugin(string pluginName, string version) {
		if (!System.Version.TryParse(version, out Version? wanted)) {
			Service.Plugin.Error($"Invalid version [{version}] in HasPlugin() call, falling back to unversioned check");
			return this.HasPlugin(pluginName);
		}
		InstalledPluginState? found = this.findPlugin(pluginName);
		if (found is null) {
			this.Log($"{pluginName} is not installed");
			return false;
		}
		if (!found.IsLoaded) {
			this.Log($"{found.Name} is installed but not loaded");
			return false;
		}
		if (found.Version.Major != wanted.Major || found.Version < wanted) {
			this.Log($"{found.Name} v{found.Version} is loaded, but not compatible with v{wanted}");
			return false;
		}
		this.Log($"{found.Name} v{found.Version} is loaded and compatible with v{wanted}");
		return true;
	}

	#endregion
}
