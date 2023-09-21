namespace PrincessRTFM.WoLua;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua;
using PrincessRTFM.WoLua.Lua.Api.Game;
using PrincessRTFM.WoLua.Ui;
using PrincessRTFM.WoLua.Ui.Chat;

using XivCommon;

public class Plugin: IDalamudPlugin {
	public const InteropAccessMode TypeRegistrationMode = InteropAccessMode.BackgroundOptimized;

	public string Name { get; } = "WoLua";
	public string Version { get; init; }
	public string Command => $"/{this.Name.ToLower()}";

	public SeString Status {
		get => this.disposed ? null! : Service.StatusLine.Text ?? string.Empty;
		set {
			if (!this.disposed && Service.StatusLine is not null)
				Service.StatusLine.Text = value;
		}
	}

	public WindowSystem Windows { get; } = new();
	private readonly MainWindow mainWindow;
	private readonly DebugWindow debugWindow;

	private Task scriptScanner = Task.CompletedTask;

	static Plugin() {
		UserData.RegisterAssembly(typeof(Plugin).Assembly, true);
		Script.GlobalOptions.RethrowExceptionNested = true;
		Script.GlobalOptions.Platform = new LimitedPlatformAccessor();
	}

	public Plugin(DalamudPluginInterface i) {
		this.Version = FileVersionInfo.GetVersionInfo(this.GetType().Assembly.Location).ProductVersion ?? "?.?.?";
		if (i.Create<Service>(this, i.GetPluginConfig() ?? new PluginConfiguration(), new XivCommonBase()) is null)
			throw new ApplicationException("Failed to initialise service container");
		Service.Sounds = new();
		Service.Hooks = new();

		Service.CommandManager.AddHandler(this.Command, new(this.OnCommand) {
			ShowInHelp = true,
			HelpMessage = $"The core {this.Name} command. Use alone to display the main interface and help window.",
		});

		this.mainWindow = new();
		this.debugWindow = new();
		this.Windows.AddWindow(this.mainWindow);
		this.Windows.AddWindow(this.debugWindow);

		Service.Interface.UiBuilder.OpenConfigUi += this.ToggleConfigUi;
		Service.Interface.UiBuilder.Draw += this.Windows.Draw;

		Task.Run(this.delayedPluginSetup);
	}

	private void delayedPluginSetup() {
		PlayerApi.InitialiseEmotes();
		this.Rescan();
	}

	public void ToggleConfigUi() {
		if (this.disposed)
			return;

		this.mainWindow.IsOpen ^= true;
	}

	public void OnCommand(string command, string argline) {
		if (this.disposed)
			return;

		string[] args = argline.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		string subcmd = args.Length >= 1 ? args[0] : string.Empty;
		switch (subcmd) {
			case "":
			case "config":
			case "cfg":
				this.ToggleConfigUi();
				break;
			case "reload":
			case "load":
			case "rescan":
			case "scan":
				this.Rescan();
				break;
			case "exec":
			case "execute":
			case "call":
			case "invoke":
			case "run":
				if (args.Length < 2) {
					this.Error("Invalid usage. You must pass a command name, optionally followed by any parameters.");
				}
				else {
					string name = args[1];
					string parameters = argline
						.TrimStart() // skip leading spaces
						[subcmd.Length..] // cut off the subcommand
						.TrimStart() // skip spaces between the subcommand and the target name
						[name.Length..]; // cut off the target name, plus one for a single space - if the user writes more, the command must handle it
					if (parameters.Length > 0 && parameters.StartsWith(' '))
						parameters = parameters[1..];
					this.Invoke(name, parameters);
				}
				break;
			case "commands":
			case "list":
			case "ls": // bit of an easter egg for programmers, I guess
				if (Service.Scripts.Count > 0) {
					this.Print(
						$"There are {Service.Scripts.Count} command{(Service.Scripts.Count == 1 ? "" : "s")}:"
							+ string.Join("\n", Service.Scripts.Keys.Select(s => $"- {s}"))
					);
				}
				else {
					this.Print("There are no commands loaded.");
				}
				break;
			case "flush":
				Service.Configuration.Save();
				this.Print("Flushed configuration to disk");
				break;
			case "debug":
				this.debugWindow.IsOpen = true;
				break;
			default:
				this.Error($"Unknown command \"{subcmd}\"");
				break;
		}
	}

	public void Invoke(string name, string parameters) {
		if (this.disposed)
			return;

		string[] tries = new string[] {
			name,
			name.ToLower(),
			name.ToUpper(),
			name.ToLowerInvariant(),
			name.ToUpperInvariant(),
		};
		ScriptContainer? cmd = null;
		foreach (string attempt in tries) {
			if (Service.Scripts.TryGetValue(attempt, out cmd))
				break;
		}
		if (cmd is not null) {
			cmd.Invoke(parameters);
		}
		else {
			this.Error($"No such command \"{name}\" exists.");
		}
	}

	private void scanScripts() {

		this.Status = StatusText.LoadingScripts;

		string path = Service.Configuration.BasePath;

		if (File.Exists(path)) {
			this.Error($"Base script folder \"{path}\" is actually a file");
			return;
		}
		else if (!Directory.Exists(path)) {
			try {
				Directory.CreateDirectory(path);
			}
			catch (Exception ex) {
				this.Error($"Unable to create base script folder \"{path}\"", ex);
				return;
			}
		}

		clearCommands();
		PluginLog.Information($"[{LogTag.ScriptLoader}:{LogTag.PluginCore}] Scanning root script directory {path}");
		bool direct = Service.Configuration.RegisterDirectCommands;
		foreach (string dir in Directory.EnumerateDirectories(path)) {
			string file = Path.Combine(dir, "command.lua");
			string name = new DirectoryInfo(dir).Name;
			string slug = ScriptContainer.NameToSlug(name);
			if (Service.Scripts.ContainsKey(slug)) {
				this.Error($"Duplicate script invocation name {slug} (for {name})");
				continue;
			}
			if (File.Exists(file)) {
				PluginLog.Information($"[{LogTag.ScriptLoader}:{slug}] Loading {file}");
				ScriptContainer script = new(file, name, slug);
				PluginLog.Information($"[{LogTag.ScriptLoader}:{slug}] Registering script container for {slug}");
				Service.Scripts.Add(slug, script);
				if (direct && script.Active) {
					if (!script.RegisterCommand())
						this.Error($"Unable to register //{script.InternalName} - is it already in use?");
				}
				if (!script.Ready) {
					PluginLog.Error($"[{LogTag.ScriptLoader}:{slug}] Script does not have a registered callback!");
				}
			}
			else {
				PluginLog.Error($"[{LogTag.ScriptLoader}:{slug}] Cannot load script {name}, no initialisation file exists");
			}
		}

		Service.Configuration.Save();

		this.Status = StatusText.Scripts;
	}
	public void Rescan() {
		if (this.disposed)
			return;

		lock (this) {
			if (this.scriptScanner.IsCompleted) {
				this.scriptScanner = Task.Run(this.scanScripts);
			}
		}
	}

	#region Chat

	public void Print(string message, UIForegroundPayload? msgCol = null, string? scriptOrigin = null) {
		if (this.disposed)
			return;

		List<Payload> parts = new() {
			Foreground.Self,
			new TextPayload($"[{this.Name}]"),
			Foreground.Reset,
		};
		if (!string.IsNullOrWhiteSpace(scriptOrigin)) {
			parts.AddRange(new Payload[] {
				Foreground.Script,
				new TextPayload($"[{scriptOrigin}]"),
				Foreground.Reset,
			});
		}
		parts.AddRange(new Payload[] {
			msgCol ?? Foreground.Normal,
			new TextPayload(" " + message),
			Foreground.Reset
		});
		Service.ChatGui.Print(new SeString(parts));
	}
	public void Error(string message, Exception? cause = null, string? scriptOrigin = null) {
		if (this.disposed)
			return;

		this.Print(message, Foreground.Error, scriptOrigin);
		if (cause is not null)
			PluginLog.Error(cause, message);
		else
			PluginLog.Error(message);
	}

	#endregion

	public void NYI() => this.Error("This feature is not yet implemented.");

	private static void clearCommands() {

		PluginLog.Information($"[{LogTag.PluginCore}] Disposing all loaded scripts");
		ScriptContainer[] scripts = Service.Scripts?.Values?.ToArray() ?? Array.Empty<ScriptContainer>();
		foreach (ScriptContainer script in scripts) {
			script?.Dispose();
		}
		PluginLog.Information($"[{LogTag.PluginCore}] Clearing all loaded scripts");
		Service.Scripts?.Clear();
	}

	#region Disposable
	private bool disposed = false;
	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		this.Status = StatusText.Disposing;
		clearCommands();

		if (disposing) {
			PluginLog.Information($"[{LogTag.PluginCore}] Flushing configuration to disk");
			Service.Configuration.Save();
			Service.Hooks.Dispose();
			Service.Common.Dispose();
			Service.Interface.UiBuilder.Draw -= this.Windows.Draw;
			Service.Interface.UiBuilder.OpenConfigUi -= this.ToggleConfigUi;
			Service.CommandManager.RemoveHandler(this.Command);
			Service.StatusLine.Dispose();
		}

		PluginLog.Information($"[{LogTag.PluginCore}] {this.Name} unloaded successfully!");
	}

	~Plugin() {
		this.Dispose(false);
	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
