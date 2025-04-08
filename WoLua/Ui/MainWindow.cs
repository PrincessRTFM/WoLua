using System.Diagnostics;
using System.IO;

using ImGuiNET;

using PrincessRTFM.WoLua.Lua;
using PrincessRTFM.WoLua.Lua.Docs;

namespace PrincessRTFM.WoLua.Ui;

internal class MainWindow: BaseWindow {
	public const ImGuiWindowFlags CreationFlags = ImGuiWindowFlags.None
		| ImGuiWindowFlags.AlwaysAutoResize;
	public const int Width = 700;

	private string basePath;
	private string directInvocationCommandPrefix;
	private bool registerCommands;
	private bool experimentalPathNormalisation;

	public MainWindow() : base($"{Plugin.Name} v{Service.Plugin.Version}##MainWindow", CreationFlags) {
		this.SizeConstraints = new() {
			MinimumSize = new(Width, 100),
			MaximumSize = new(Width, 800),
		};
		this.basePath = Service.Configuration.BasePath;
		this.directInvocationCommandPrefix = Service.Configuration.DirectInvocationCommandPrefix;
		this.registerCommands = Service.Configuration.RegisterDirectCommands;
		this.experimentalPathNormalisation = Service.Configuration.ExperimentalPathNormalisation;
	}

	public override void OnOpen() => this.basePath = Service.Configuration.BasePath;

	public override void Draw() {
		string
			exampleScriptName = "MyScript",
			exampleScriptNamePrefixed = $"{Plugin.Name}.{exampleScriptName}",

			exampleScriptSlug = ScriptContainer.NameToSlug(exampleScriptName).ToLower(),
			exampleScriptSlugPrefixed = ScriptContainer.NameToSlug(exampleScriptNamePrefixed).ToLower(),
			exampleNormalisedSlug = ScriptContainer.NameToSlug(exampleScriptName, true).ToLower(),
			exampleNormalisedSlugPrefixed = ScriptContainer.NameToSlug(exampleScriptNamePrefixed, true).ToLower(),

			exampleScriptCall = $"{Plugin.Command} call {exampleScriptSlug}",
			exampleScriptCallPrefixed = $"{Plugin.Command} call {exampleScriptSlugPrefixed}",
			exampleNormalisedScriptCall = $"{Plugin.Command} call {exampleNormalisedSlug}",
			exampleNormalisedScriptCallPrefixed = $"{Plugin.Command} call {exampleNormalisedSlugPrefixed}",

			exampleShortCall = $"/{this.directInvocationCommandPrefix}{exampleScriptSlug}",
			exampleShortCallPrefixed = $"/{this.directInvocationCommandPrefix}{exampleScriptSlugPrefixed}",
			exampleNormalisedShortCall = $"/{this.directInvocationCommandPrefix}{exampleNormalisedSlug}",
			exampleNormalisedShortCallPrefixed = $"/{this.directInvocationCommandPrefix}{exampleNormalisedSlugPrefixed}",

			exampleCall = Service.Configuration.RegisterDirectCommands
				? exampleShortCall
				: exampleScriptCall,
			exampleCallPrefixed = Service.Configuration.RegisterDirectCommands
				? exampleShortCallPrefixed
				: exampleScriptCallPrefixed,
			exampleNormalisedCall = Service.Configuration.RegisterDirectCommands
				? exampleNormalisedShortCall
				: exampleNormalisedScriptCall,
			exampleNormalisedCallPrefixed = Service.Configuration.RegisterDirectCommands
				? exampleNormalisedShortCallPrefixed
				: exampleNormalisedScriptCallPrefixed;

		ImGuiStylePtr style = ImGui.GetStyle();
		float imguiPadding = style.CellPadding.X * 2;
		float imguiSpacing = style.ItemSpacing.X;

		ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);

		if (Section("Introduction")) {
			Textline($"Welcome to {Plugin.Name}, where you write your own chat commands!");

			Textline("Until now, the only way to make your own context-aware chat commands withoug writing a whole entire plugin was to make a macro using TinyCommands."
				+ " Unfortunately, the conditional commands could be hard to work with, and weren't easy to extend. Chaining them ran the risk of timing issues in the game, too."
				+ $" {Plugin.Name} aims to fix this, by allowing you to write your own logic in lua, creating a script that interfaces with the game itself.");

			Textline($"If you don't know lua, don't worry! Not only is the language itself fairly easy to learn, but you can get premade command scripts from other users for use in {Plugin.Name}."
				+ " As always though, be cautious when running unknown and untrusted code. I've done my best to prevent people from escaping the sandbox, but scripts still have access to your game."
				+ " A malicious script could easily spam shout chat with bannable messages, for example.");
		}

		if (Section("Usage")) {
			Textline($"{Plugin.Name} automatically scans for command scripts on startup, when you change the base folder path, and when you use the \"{Plugin.Command} reload\" command."
				+ " Any command scripts found are loaded immediately, and any load errors are printed to your local chatlog.");

			Textline($"Once a command script is loaded, you can use it with the \"{Plugin.Command} call <name> [<optional arguments...>]\" command, with the name of the subfolder it's in."
				+ " Any following text will be passed to the command itself.");

			Textline($"For a list of commands, you can use \"{Plugin.Command} list\" to output the names of every loaded command.");
		}

		if (Section("Documentation")) {
			Textline($"Since the documentation for writing {Plugin.Name} scripts is necessarily so extensive, it's located online, on the plugin repository.");

			if (ImGui.Button("Open documentation page")) {
				Process.Start(new ProcessStartInfo("https://github.com/VariableVixen/WoLua/tree/master/docs#wolua-scripting") { UseShellExecute = true });
			}

			Textline($"However, if you're writing your own scripts, you can use \"{Plugin.Command} api\" to generate an API definition file in {LuadocGenerator.ApiDefinitionFilePath}"
				+ " which can be used as a reference in your IDE to provide documentation, autocompletion, type checking, etc. You can simply re-run the command to regenerate the file"
				+ " at any time, such as after an update.");
			Textline($"Note that the generated API definition file does NOT produce a file that will allow you to run your script outside of {Plugin.Name}. It is ONLY for use"
				+ " with an IDE to provide a live reference.");
			Textline("Also note that it's possible some IDEs or lua editor plugins may not understand the produced format. It was designed for use with the VSCode lua language server plugin,"
				+ " so in theory it should be relatively standard, but I cannot control IDE/plugin documentation parsing.");
		}

		if (Section("Settings")) {

			Textline("Base lua script folder:");
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - (ImGui.CalcTextSize("Rescan").X + imguiPadding + imguiSpacing));
			ImGui.InputTextWithHint("###BaseFolderPath", PluginConfiguration.Defaults.BasePath, ref this.basePath, byte.MaxValue);
			if (ImGui.IsItemDeactivatedAfterEdit()) {
				Service.Configuration.BasePath = this.basePath;
				Service.Configuration.Save();
				Service.ScriptManager.Rescan();
			}
			ImGui.SameLine();
			if (ImGui.Button("Rescan"))
				Service.ScriptManager.Rescan();
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline("This is where all of your lua command scripts will go.", 0);
			ImGui.Unindent();
			ImGui.EndDisabled();

			Textline();
			if (ImGui.Checkbox("Try to register direct commands for each script?###RegisterDirectCommands", ref this.registerCommands)) {
				Service.Configuration.RegisterDirectCommands = this.registerCommands;
				Service.Configuration.Save();
				Service.ScriptManager.Rescan();
			}
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline($"If this is enabled, {Plugin.Name} will try to register commands with Dalamud for each loaded script.", 0);
			Textline($"This would let you to skip using `{Plugin.Command} call` when running scripts.", 0);
			Textline("This will FAIL for any commands that are already in use, such as by other plugins.", 0);
			Textline("Due to how Dalamud commands work, this will never override built-in game commands.", 0);
			Textline($"In order to reduce collisions, each command is registered with a prefix, set below.", 0);
			ImGui.Unindent();
			ImGui.EndDisabled();

			Textline();
			Textline("Direct script command prefix:");
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			ImGui.BeginDisabled(!Service.Configuration.RegisterDirectCommands);
			if (ImGui.InputTextWithHint("###DirectInvocationCommandPrefix", PluginConfiguration.Defaults.DirectInvocationCommandPrefix, ref this.directInvocationCommandPrefix, byte.MaxValue))
				this.directInvocationCommandPrefix = this.directInvocationCommandPrefix.Replace(" ", "");
			ImGui.EndDisabled();
			if (ImGui.IsItemDeactivatedAfterEdit()) {
				if (string.IsNullOrWhiteSpace(this.directInvocationCommandPrefix))
					this.directInvocationCommandPrefix = PluginConfiguration.Defaults.DirectInvocationCommandPrefix;
				Service.Configuration.DirectInvocationCommandPrefix = this.directInvocationCommandPrefix;
				Service.Configuration.Save();
				Service.ScriptManager.Rescan();
			}
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline("When direct script commands are enabled, they will be registered with this prefix before the script name.", 0);
			Textline($"For example, a script named `{exampleScriptNamePrefixed}` will get the command `{exampleShortCallPrefixed}` as a shortcut.", 0);
			Textline("This is done in order to reduce collisions with other plugins and vanilla game commands.", 0);
			Textline("For that reason, this value CANNOT contain spaces or be left empty.", 0);
			ImGui.Unindent();
			ImGui.EndDisabled();

			Textline();
			if (ImGui.Checkbox("Enable experimental path normalisation?###ExperimentalPathNormalisation", ref this.experimentalPathNormalisation)) {
				Service.Configuration.ExperimentalPathNormalisation = this.experimentalPathNormalisation;
				Service.Configuration.Save();
				Service.ScriptManager.Rescan();
			}
			ImGui.Indent();
			ImGui.BeginDisabled();
			Textline("If this is enabled, script paths will undergo additional transformations to produce a \"cleaner\" name.", 0);
			Textline($"For example, leading `{Plugin.Name.ToLower()}.` and trailing `.{Plugin.Name.ToLower()}` strings will be removed.", 0);
			Textline($"This means that a script folder named `{exampleScriptNamePrefixed}` will be called via `{exampleNormalisedCallPrefixed}`.", 0);
			Textline("This feature is still being experimented with, and may be subject to change at any time.", 0);
			Textline("Any scripts affected by this option will LOSE existing script storage values unless the file is manually renamed!", 0);
			ImGui.Unindent();
			ImGui.EndDisabled();

			Textline();
			Separator();
			ImGui.BeginDisabled();
			Textline("Given your current settings, a script named", 0);
			ImGui.Indent();
			Textline(exampleScriptNamePrefixed, 0);
			ImGui.Unindent();
			Textline("will be called via", 0);
			ImGui.Indent();
			Textline(exampleScriptCallPrefixed, 0);
			if (Service.Configuration.RegisterDirectCommands)
				Textline(exampleShortCallPrefixed, 0);
			ImGui.Unindent();
			ImGui.EndDisabled();
		}

		ImGui.PopTextWrapPos();
	}
}
