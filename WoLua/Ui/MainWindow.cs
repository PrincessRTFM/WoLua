namespace PrincessRTFM.WoLua.Ui;

using System.Diagnostics;

using ImGuiNET;

using PrincessRTFM.WoLua.Lua;

internal class MainWindow: BaseWindow {
	public const ImGuiWindowFlags CreationFlags = ImGuiWindowFlags.None
		| ImGuiWindowFlags.AlwaysAutoResize;
	public const int Width = 650;

	private string basePath;
	private bool registerCommands;
	private bool experimentalPathNormalisation;

	public MainWindow() : base($"{Service.Plugin.Name}##MainWindow", CreationFlags) {
		this.SizeConstraints = new() {
			MinimumSize = new(Width, 100),
			MaximumSize = new(Width, 800),
		};
		this.basePath = Service.Configuration.BasePath;
		this.registerCommands = Service.Configuration.RegisterDirectCommands;
	}

	public override void OnOpen() => this.basePath = Service.Configuration.BasePath;

	public override void Draw() {
		ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);

		if (Section("Introduction")) {
			Textline($"Welcome to {Service.Plugin.Name}, where you write your own chat commands!");

			Textline("Until now, the only way to make your own context-aware chat commands withoug writing a whole entire plugin was to make a macro using TinyCommands."
				+ " Unfortunately, the conditional commands could be hard to work with, and weren't easy to extend. Chaining them ran the risk of timing issues in the game, too."
				+ $" {Service.Plugin.Name} aims to fix this, by allowing you to write your own logic in lua, creating a script that interfaces with the game itself.");

			Textline($"If you don't know lua, don't worry! Not only is the language itself fairly easy to learn, but you can get premade command scripts from other users for use in {Service.Plugin.Name}."
				+ " As always though, be cautious when running unknown and untrusted code. I've done my best to prevent people from escaping the sandbox, but scripts still have access to your game."
				+ " A malicious script could easily spam shout chat with bannable message, for example.");
		}

		if (Section("Usage")) {
			Textline($"{Service.Plugin.Name} automatically scans for command scripts on startup, when you change the base folder path, and when you use the \"{Service.Plugin.Command} reload\" command."
				+ " Any command scripts found are loaded immediately, and any load errors are printed to your local chatlog.");

			Textline($"Once a command script is loaded, you can use it with the \"{Service.Plugin.Command} call <name> [<optional arguments...>]\" command, with the name of the subfolder it's in."
				+ " Any following text will be passed to the command itself.");

			Textline($"For a list of commands, you can use \"{Service.Plugin.Command} list\" to output the names of every loaded command.");
		}

		if (Section("Documentation")) {
			Textline($"Since the documentation for writing {Service.Plugin.Name} scripts is necessarily so extensive, it's located online, on the plugin repository.");

			if (ImGui.Button("Open documentation page")) {
				Process.Start(new ProcessStartInfo("https://github.com/PrincessRTFM/WoLua/tree/master/docs") { UseShellExecute = true });
			}
		}

		if (Section("Settings")) {

			Textline("Base lua script folder:");
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			ImGui.InputTextWithHint("###BaseFolderPath", PluginConfiguration.Defaults.BasePath, ref this.basePath, byte.MaxValue);
			if (ImGui.IsItemDeactivatedAfterEdit()) {
				Service.Configuration.BasePath = this.basePath;
				Service.Configuration.Save();
				Service.Plugin.Rescan();
			}
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline("This is where all of your lua command scripts will go.", 0);
			ImGui.Unindent();
			ImGui.EndDisabled();

			Textline();
			if (ImGui.Checkbox("Try to register direct commands for each script?###RegisterDirectCommands", ref this.registerCommands)) {
				Service.Configuration.RegisterDirectCommands = this.registerCommands;
				Service.Configuration.Save();
				Service.Plugin.Rescan();
			}
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline($"If this is enabled, {Service.Plugin.Name} will try to register commands with Dalamud for each loaded script.", 0);
			Textline($"This would let you to skip using `{Service.Plugin.Command} call` when running scripts.", 0);
			Textline("This will FAIL for any commands that are already in use, such as by other plugins.", 0);
			Textline("Due to how Dalamud commands work, this will never override built-in game commands.", 0);
			Textline("However, each script shortcut command is registered with TWO leading slashes, so collisions should be minimal.", 0);
			Textline("For example, a script named `myscript` will get the command `//myscript` as a shortcut.", 0);
			ImGui.Unindent();
			ImGui.EndDisabled();

			Textline();
			if (ImGui.Checkbox("Enable experimental path normalisation?###ExperimentalPathNormalisation", ref this.experimentalPathNormalisation)) {
				Service.Configuration.ExperimentalPathNormalisation = this.experimentalPathNormalisation;
				Service.Configuration.Save();
				Service.Plugin.Rescan();
			}
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline("If this is enabled, script paths will undergo additional transformations to produce a \"cleaner\" name.", 0);
			Textline($"For example, leading \"{Service.Plugin.Name.ToLower()}.\" and trailing \".{Service.Plugin.Name.ToLower()}\" strings will be removed.", 0);
			Textline($"This means that a script folder named \"{Service.Plugin.Name}.MyScript\" will be called via \"{Service.Plugin.Command} call myscript\".", 0);
			Textline("This feature is still being experimented with, and may be subject to change at any time.");
			Textline("Any scripts affected by this option will LOSE existing script storage values unless the file is manually renamed!");
			ImGui.Unindent();
			ImGui.EndDisabled();
		}

		ImGui.PopTextWrapPos();
	}
}
