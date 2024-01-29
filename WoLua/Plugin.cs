using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;

using Lumina.Excel.GeneratedSheets;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua;
using PrincessRTFM.WoLua.Lua.Api.Game;
using PrincessRTFM.WoLua.Lua.Docs;
using PrincessRTFM.WoLua.Ui;
using PrincessRTFM.WoLua.Ui.Chat;

using XivCommon;

namespace PrincessRTFM.WoLua;

public class Plugin: IDalamudPlugin {
	public const InteropAccessMode TypeRegistrationMode = InteropAccessMode.BackgroundOptimized;
	public const string Name = "WoLua";

	public static string Command => $"/{Name.ToLower()}";

	public string Version { get; init; }

	public SeString ShortStatus {
		get => this.disposed ? null! : Service.StatusLine?.Text ?? string.Empty;
		set {
			if (!this.disposed && Service.StatusLine is not null)
				Service.StatusLine.Text = value;
		}
	}
	[AllowNull]
	public SeString FullStatus {
		get => this.disposed ? null! : Service.StatusLine?.Tooltip ?? string.Empty;
		set {
			if (!this.disposed && Service.StatusLine is not null)
				Service.StatusLine.Tooltip = value;
		}
	}

	public WindowSystem Windows { get; } = new();
	private readonly MainWindow mainWindow;
	private readonly DebugWindow debugWindow;

	public SingleExecutionTask ScriptScanner { get; init; }
	public SingleExecutionTask DocumentationGenerator { get; init; }

	static Plugin() {
		UserData.RegisterAssembly(typeof(Plugin).Assembly, true);
		Script.GlobalOptions.RethrowExceptionNested = true;
		Script.GlobalOptions.Platform = new LimitedPlatformAccessor();
	}

	public Plugin(DalamudPluginInterface i) {
		using MethodTimer logtimer = new();

		this.ScriptScanner = new(this.scanScripts);
		this.DocumentationGenerator = new(this.writeLuaDocs);

		this.Version = FileVersionInfo.GetVersionInfo(this.GetType().Assembly.Location).ProductVersion ?? "?.?.?";
		if (i.Create<Service>(this, i.GetPluginConfig() ?? new PluginConfiguration(), new XivCommonBase(i)) is null)
			throw new ApplicationException("Failed to initialise service container");
		Service.Sounds = new();
		Service.Hooks = new();

		Service.CommandManager.AddHandler(Command, new(this.OnCommand) {
			ShowInHelp = true,
			HelpMessage = $"The core {Name} command. Use alone to display the main interface and help window.",
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
		WeatherWrapper.LoadGameData();
		MountWrapper.LoadGameData();
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
				if (!Service.Scripts.IsEmpty) {
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
			case "make-docs":
			case "make-api-ref":
			case "gen-docs":
			case "api":
				this.DocumentationGenerator.Run();
				break;
			case "query":
				if (Service.ClientState.LocalContentId == 0 || Service.ClientState.LocalPlayer is null) {
					this.Error("No character is loaded. You are probably not logged in.");
					break;
				}
				if (args.Length >= 2) {
					switch (args[1].ToLower()) {
						case "id":
							this.Print($"Your current character ID is {Service.ClientState.LocalContentId}.");
							break;
						case "class":
						case "job":
							if (Service.ClientState.LocalPlayer.ClassJob.GameData is ClassJob job) {
								this.Print($"You current job is {job.RowId} ({job.Abbreviation}, {job.Name}).");
							}
							else {
								this.Error("Cannot determine class/job details.");
							}
							break;
						case "mount":
							unsafe {
								Character* player = (Character*)Service.ClientState.LocalPlayer.Address;
								if (!player->IsMounted()) {
									this.Print("You are not mounted.");
								}
								else {
									Character.MountContainer mount = player->Mount;
									ushort id = mount.MountId;
									this.Print($"You are currently using mount {id} ({MountWrapper.mountArticles[id].ToLower()} {MountWrapper.mountNames[id]}).");
								}
							}
							break;
						case "zone":
							this.Print($"Current map zone is {Service.ClientState.TerritoryType}.");
							break;
						case "weather":
							unsafe {
								EnvManager* env = EnvManager.Instance();
								byte id = env is null ? (byte)0 : env->ActiveWeather;
								this.Print($"Current weather is {id} ({WeatherWrapper.weatherNames[id]}, {WeatherWrapper.weatherDescriptions[id]}).");
							}
							break;
						default:
							this.Error("Unknown query.");
							break;
					}
				}
				else {
					this.Print("Valid queries: id, class/job, mount, zone, weather.");
				}
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
		using MethodTimer logtimer = new();

		this.ShortStatus = StatusText.LoadingScripts;
		this.FullStatus = StatusText.TooltipLoadingScripts;

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
		Service.Log.Information($"[{LogTag.ScriptLoader}:{LogTag.PluginCore}] Scanning root script directory {path}");
		string[] dirs = Directory.GetDirectories(path);
		Service.Log.Information($"[{LogTag.ScriptLoader}:{LogTag.PluginCore}] Found {dirs.Length} script director{(dirs.Length == 1 ? "y" : "ies")}");
		bool direct = Service.Configuration.RegisterDirectCommands;
		foreach (string dir in dirs) {
			string file = Path.Combine(dir, "command.lua");
			string name = new DirectoryInfo(dir).Name;
			string slug = ScriptContainer.NameToSlug(name);
			if (Service.Scripts.ContainsKey(slug)) {
				this.Error($"Duplicate script invocation name {slug} (for {name})");
				continue;
			}
			if (File.Exists(file)) {
				Service.Log.Information($"[{LogTag.ScriptLoader}:{slug}] Loading {file}");
				ScriptContainer script = new(file, name, slug);
				Service.Log.Information($"[{LogTag.ScriptLoader}:{slug}] Registering script container for {slug}");
				Service.Scripts.TryAdd(slug, script);
				if (direct && script.Active) {
					if (!script.RegisterCommand())
						this.Error($"Unable to register //{script.InternalName} - is it already in use?");
				}
				if (!script.Ready) {
					Service.Log.Error($"[{LogTag.ScriptLoader}:{slug}] Script does not have a registered callback!");
				}
			}
			else {
				Service.Log.Error($"[{LogTag.ScriptLoader}:{slug}] Cannot load script {name}, no initialisation file exists");
			}
		}

		Service.Log.Info($"[{LogTag.ScriptLoader}] Finished loading all scripts ({Service.Scripts.Count} found)");

		Service.Configuration.Save();

		this.ShortStatus = StatusText.Scripts;
		this.FullStatus = StatusText.TooltipLoaded;
	}
	public void Rescan() {
		if (this.disposed)
			return;

		this.ScriptScanner.Run();
	}

	private void writeLuaDocs() {
		using MethodTimer timer = new();
		string contents;
		try {
			contents = LuadocGenerator.GenerateLuadoc();
		}
		catch (Exception e) {
			this.Error("Failed to generate lua API reference", e);
			return;
		}
		try {
			string path = Path.Combine(Service.Configuration.BasePath, "api.lua");
			File.WriteAllText(path, contents);
			this.Print($"Lua API reference written to {path}");
		}
		catch (Exception e) {
			this.Error("Failed to write lua API definition file", e);
			return;
		}
	}

	#region Chat

	public void Print(string message, UIForegroundPayload? msgCol = null, string? scriptOrigin = null) {
		if (this.disposed)
			return;

		List<Payload> parts = new() {
			Foreground.Self,
			new TextPayload($"[{Name}]"),
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
			Service.Log.Error(cause, message);
		else
			Service.Log.Error(message);
	}

	#endregion

	public void NYI() => this.Error("This feature is not yet implemented.");

	private static void clearCommands() {
		using MethodTimer logtimer = new();

		Service.Log.Information($"[{LogTag.PluginCore}] Disposing all loaded scripts");
		ScriptContainer[] scripts = Service.Scripts?.Values?.ToArray() ?? Array.Empty<ScriptContainer>();
		foreach (ScriptContainer script in scripts) {
			script?.Dispose();
		}
		Service.Log.Information($"[{LogTag.PluginCore}] Clearing all loaded scripts");
		Service.Scripts?.Clear();
	}

	#region Disposable
	private bool disposed = false;
	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;
		using MethodTimer logtimer = new();

		this.ShortStatus = StatusText.Disposing;
		this.FullStatus = StatusText.TooltipDisposing;
		clearCommands();

		if (disposing) {
			Service.Log.Information($"[{LogTag.PluginCore}] Flushing configuration to disk");
			Service.Configuration.Save();
			Service.Hooks.Dispose();
			Service.Common.Dispose();
			Service.Interface.UiBuilder.Draw -= this.Windows.Draw;
			Service.Interface.UiBuilder.OpenConfigUi -= this.ToggleConfigUi;
			Service.CommandManager.RemoveHandler(Command);
			Service.StatusLine.Dispose();
		}

		Service.Log.Information($"[{LogTag.PluginCore}] {Name} unloaded successfully!");
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
