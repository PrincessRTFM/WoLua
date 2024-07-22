using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua;

namespace PrincessRTFM.WoLua;

public class ScriptManager: IDisposable {
	private ConcurrentDictionary<string, ScriptContainer> loadedScripts { get; } = new();
	private SingleExecutionTask scriptScanner { get; init; } = null!;

	internal ScriptManager() => this.scriptScanner = new(this.scanScripts);

	#region Public API

	public int TotalScripts => this.loadedScripts.Count;
	public int LoadedScripts => this.loadedScripts.Values.Where(c => c.Ready).Count();
	public int WorkingScripts => this.loadedScripts.Values.Where(c => c.Active).Count();
	public bool IsEmpty => this.loadedScripts.IsEmpty;

	public string[] Slugs => this.loadedScripts.Keys.ToArray();
	public ScriptContainer[] Scripts => this.loadedScripts.Values.ToArray();

	public bool FindScriptByInformalSlug(string identifier, [NotNullWhen(true)] out ScriptContainer? script) {
		script = null;
		if (this.disposed)
			return false;

		string[] tries = [
			identifier,
			identifier.ToLower(),
			identifier.ToUpper(),
			identifier.ToLowerInvariant(),
			identifier.ToUpperInvariant(),
		];
		foreach (string attempt in tries) {
			if (this.loadedScripts.TryGetValue(attempt, out script))
				break;
		}

		return script is not null;
	}

	public void Invoke(string name, string parameters) {
		if (this.disposed)
			return;

		if (this.FindScriptByInformalSlug(name, out ScriptContainer? cmd)) {
			cmd.Invoke(parameters);
		}
		else {
			Service.Plugin.Error($"No such command \"{name}\" exists.");
		}
	}

	public void Rescan() {
		if (this.disposed)
			return;

		this.scriptScanner.Run();
	}

	#endregion

	#region Internal API

	internal void ClearAllScripts() {
		using MethodTimer logtimer = new();

		Service.Log.Information($"[{LogTag.PluginCore}] Disposing all loaded scripts");
		ScriptContainer[] scripts = this.loadedScripts.Values?.ToArray() ?? [];
		foreach (ScriptContainer script in scripts) {
			script?.Dispose();
		}
		Service.Log.Information($"[{LogTag.PluginCore}] Clearing all loaded scripts");
		this.loadedScripts.Clear();
	}

	#endregion

	#region Private helper functions

	private void scanScripts() {
		using MethodTimer logtimer = new();

		Service.Plugin.ShortStatus = StatusText.LoadingScripts;
		Service.Plugin.FullStatus = StatusText.TooltipLoadingScripts;

		string path = Service.Configuration.BasePath;

		if (File.Exists(path)) {
			Service.Plugin.Error($"Base script folder \"{path}\" is actually a file");
			return;
		}
		else if (!Directory.Exists(path)) {
			try {
				Directory.CreateDirectory(path);
			}
			catch (Exception ex) {
				Service.Plugin.Error($"Unable to create base script folder \"{path}\"", ex);
				return;
			}
		}

		this.ClearAllScripts();
		Service.Log.Information($"[{LogTag.ScriptLoader}:{LogTag.PluginCore}] Scanning root script directory {path}");
		string[] dirs = Directory.GetDirectories(path);
		Service.Log.Information($"[{LogTag.ScriptLoader}:{LogTag.PluginCore}] Found {dirs.Length} script director{(dirs.Length == 1 ? "y" : "ies")}");
		bool direct = Service.Configuration.RegisterDirectCommands;
		foreach (string dir in dirs) {
			string file = Path.Combine(dir, "command.lua");
			string name = new DirectoryInfo(dir).Name;
			string slug = ScriptContainer.NameToSlug(name);
			if (this.loadedScripts.ContainsKey(slug)) {
				Service.Plugin.Error($"Duplicate script invocation name {slug} (for {name})");
				continue;
			}
			if (File.Exists(file)) {
				Service.Log.Information($"[{LogTag.ScriptLoader}:{slug}] Loading {file}");
				ScriptContainer script = new(file, name, slug);
				Service.Log.Information($"[{LogTag.ScriptLoader}:{slug}] Registering script container for {slug}");
				this.loadedScripts.TryAdd(slug, script);
				if (direct && script.Active) {
					if (!script.RegisterCommand())
						Service.Plugin.Error($"Unable to register //{script.InternalName} - is it already in use?");
				}
				if (!script.Ready) {
					Service.Log.Error($"[{LogTag.ScriptLoader}:{slug}] Script does not have a registered callback!");
				}
			}
			else {
				Service.Log.Error($"[{LogTag.ScriptLoader}:{slug}] Cannot load script {name}, no initialisation file exists");
			}
		}

		Service.Log.Info($"[{LogTag.ScriptLoader}] Finished loading all scripts ({Service.ScriptManager.TotalScripts} found)");

		Service.Configuration.Save();

		Service.Plugin.ShortStatus = StatusText.Scripts;
		Service.Plugin.FullStatus = StatusText.TooltipLoaded;
	}

	#endregion

	#region IDisposable
	private bool disposed = false;

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			this.ClearAllScripts();
		}
	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion
}
