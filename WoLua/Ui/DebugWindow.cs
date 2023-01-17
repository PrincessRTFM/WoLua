namespace PrincessRTFM.WoLua.Ui;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using ImGuiNET;

using MoonSharp.Interpreter;

using PrincessRTFM.WoLua.Lua;
using PrincessRTFM.WoLua.Lua.Api;

internal class DebugWindow: BaseWindow {
	public const ImGuiWindowFlags CreationFlags = ImGuiWindowFlags.None
		| ImGuiWindowFlags.AlwaysAutoResize;
	public const int Width = 650;

	internal static readonly HashSet<string> ignoredScriptGlobals = new() {
		"_G", // the most critical - this avoids a stack overflow crash
		"_VERSION",
		"_MOONSHARP",
		"ipairs",
		"pairs",
		"next",
		"type",
		"assert",
		"collectgarbage",
		"error",
		"tostring",
		"select",
		"tonumber",
		"print",
		"setmetatable",
		"getmetatable",
		"rawget",
		"rawset",
		"rawequal",
		"rawlen",
		"string",
		"package",
		"load",
		"loadsafe",
		"loadfile",
		"loadfilesafe",
		"dofile",
		"__require_clr_impl",
		"require",
		"table",
		"unpack",
		"pack",
		"pcall",
		"xpcall",
		"math",
		"bit32",
		"dynamic",
		"os",
		"debug",
		"json",
		"Script",
		"Game",
	};

	public DebugWindow() : base($"{Service.Plugin.Name}##DebugWindow", CreationFlags) {
		this.SizeConstraints = new() {
			MinimumSize = new(Width, 100),
			MaximumSize = new(Width, 800),
		};
	}

	public override void Draw() {
		Textline($"Base path: {Service.Configuration.BasePath}");

		Textline($"Loaded scripts: {Service.Scripts.Count}");

		Separator();

		foreach ((string name, ScriptContainer script) in Service.Scripts) {
			ImGui.Spacing();
			ImGui.Spacing();

			ImGui.PushStyleColor(
				ImGuiCol.Text,
				script.Ready
					? new Vector4(0, 1, 0, 1)
					: new Vector4(1, 0, 0, 1)
			);
			Textline($"{name} [{ApiBase.ToUsefulString(script.callback, true)}]", 0);
			ImGui.PopStyleColor();

			ImGui.Spacing();

			if (script.Ready) {
				ImGui.PushTextWrapPos(Width - (ImGui.GetStyle().WindowPadding.X * 2));

				Textline("Globals:", 0);
				ImGui.Indent();
				try {
					Script engine = script.Engine;
					Table globals = engine.Globals;
					DynValue[] globalVars = globals.Keys.ToArray();
					foreach (DynValue key in globalVars) {
						if (ignoredScriptGlobals.Contains(key.CastToString()))
							continue;
						DynValue value = globals.Get(key);
						Textline($"[{ApiBase.ToUsefulString(key, true)}] = {ApiBase.ToUsefulString(value, true)}", 0);
					}
				}
				catch (Exception e) {
					Textline(e.ToString(), 0);
				}
				ImGui.Unindent();

				ImGui.Spacing();

				Textline("Storage:", 0);
				ImGui.Indent();
				try {
					Script engine = script.Engine;
					Table globals = engine.Globals;
					ScriptApi api = script.ScriptApi;
					Table storage = api.Storage;
					DynValue[] storedVars = storage.Keys.ToArray();
					foreach (DynValue key in storedVars) {
						DynValue value = storage.Get(key);
						Textline($"[{ApiBase.ToUsefulString(key, true)}] = {ApiBase.ToUsefulString(value, true)}", 0);
					}
				}
				catch (Exception e) {
					Textline(e.ToString(), 0);
				}
				ImGui.Unindent();

				ImGui.PopTextWrapPos();
			}

			ImGui.Spacing();
			ImGui.Spacing();
		}
	}
}
