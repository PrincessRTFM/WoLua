using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using PrincessRTFM.WoLua.Ui;
using PrincessRTFM.WoLua.Ui.Chat;

using XivCommon;

namespace PrincessRTFM.WoLua;

public class Plugin: IDalamudPlugin {
	public const InteropAccessMode TypeRegistrationMode = InteropAccessMode.BackgroundOptimized;
	public const string Name = "WoLua";

	public static string Command { get; } = $"/{Name.ToLower()}";

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

	static Plugin() {
		UserData.RegisterAssembly(typeof(Plugin).Assembly, true);
		Script.GlobalOptions.RethrowExceptionNested = true;
		Script.GlobalOptions.Platform = new LimitedPlatformAccessor();
	}

	#region Initialisation and plugin setup

	public Plugin(IDalamudPluginInterface i) {
		using MethodTimer logtimer = new();

		this.Version = FileVersionInfo.GetVersionInfo(i.AssemblyLocation.FullName).ProductVersion ?? "?.?.?";
		if (i.Create<Service>(this, i.GetPluginConfig() ?? new PluginConfiguration()) is null)
			throw new ApplicationException("Failed to initialise service container");

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
		Service.ScriptManager.Rescan();
		Service.DocumentationGenerator.Run();
	}

	#endregion

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
				Service.ScriptManager.Rescan();
				break;
			case "exec":
			case "execute":
			case "call":
			case "invoke":
			case "run":
				if (args.Length < 2) {
					this.Error("Invalid usage. You must pass a script name, optionally followed by any parameters.");
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
					Service.ScriptManager.Invoke(name, parameters);
				}
				break;
			case "info":
			case "check":
				if (args.Length < 2) {
					this.Error("Invalid usage. You must pass a script name.");
				}
				else if (Service.ScriptManager.FindScriptByInformalSlug(args[1], out ScriptContainer? script) && !script.ReportError()) {
					this.Print($"\"{script.PrettyName}\" ({script.InternalName}) has {script.ActionQueue.Count} queued action{(script.ActionQueue.Count == 1 ? "" : "s")}");
				}
				else {
					this.Error($"No script could be found by the identifier {args[1]}.");
				}
				break;
			case "halt":
			case "stop":
			case "clear":
				if (args.Length < 2) {
					this.Error("Invalid usage. You must pass a script name.");
				}
				else if (Service.ScriptManager.FindScriptByInformalSlug(args[1], out ScriptContainer? script) && !script.ReportError()) {
					int had = script.ActionQueue.Count;
					script.ActionQueue.Clear();
					this.Print($"Cleared {had} action{(had == 1 ? "" : "s")} from {script.PrettyName}'s queue.");
				}
				else {
					this.Error($"No script could be found with the informal identifier {args[1]}.");
				}
				break;
			case "halt-all":
			case "stop-all":
			case "clear-all":
			case "haltall":
			case "stopall":
			case "clearall":
				foreach (ScriptContainer script in Service.ScriptManager.Scripts) {
					if (!script.ReportError()) {
						int had = script.ActionQueue.Count;
						script.ActionQueue.Clear();
						this.Print($"Cleared {had} action{(had == 1 ? "" : "s")} from {script.PrettyName}'s queue.");
					}
				}
				break;
			case "commands":
			case "list":
			case "ls": // bit of an easter egg for programmers, I guess
				if (!Service.ScriptManager.IsEmpty) {
					this.Print($"There {(Service.ScriptManager.TotalScripts == 1 ? "is" : "are")} {Service.ScriptManager.TotalScripts} command{(Service.ScriptManager.TotalScripts == 1 ? "" : "s")}:\n"
						+ string.Join("\n", Service.ScriptManager.Slugs.Select(s => $"- {s}")));
				}
				else {
					this.Print("There are no commands loaded.");
				}
				break;
			case "flush":
				Service.Configuration.Save();
				break;
			case "debug":
				this.debugWindow.IsOpen = true;
				break;
			case "make-docs":
			case "make-api-ref":
			case "gen-docs":
			case "api":
				Service.DocumentationGenerator.Run();
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
								this.Print($"You current job is {job.RowId} ({job.Abbreviation}, {job.Name}) at level {Service.ClientState.LocalPlayer.Level}.");
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
									MountContainer mount = player->Mount;
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

	#region Chat

	public void Print(string message, UIForegroundPayload? msgCol = null, string? scriptOrigin = null) {
		if (this.disposed)
			return;

		List<Payload> parts = [
			Foreground.Self,
			new TextPayload($"[{Name}]"),
			Foreground.Reset,
		];
		if (!string.IsNullOrWhiteSpace(scriptOrigin)) {
			parts.AddRange([
				Foreground.Script,
				new TextPayload($"[{scriptOrigin}]"),
				Foreground.Reset,
			]);
		}
		parts.AddRange([
			msgCol ?? Foreground.Normal,
			new TextPayload(" " + message),
			Foreground.Reset
		]);
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

	#region Disposable
	private bool disposed = false;

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;
		using MethodTimer logtimer = new();

		if (disposing) {
			Service.CommandManager.RemoveHandler(Command);
			Service.Configuration.Save();
			Service.ScriptManager.Dispose();
			Service.Hooks.Dispose();
			Service.Common?.Dispose();
			Service.Interface.UiBuilder.Draw -= this.Windows.Draw;
			Service.Interface.UiBuilder.OpenConfigUi -= this.ToggleConfigUi;
			Service.StatusLine.Remove();
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
