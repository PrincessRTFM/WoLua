namespace PrincessRTFM.WoLua.Lua;

using System.Diagnostics;
using System.IO;

using Dalamud.Logging;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

public class ScriptLoader: IScriptLoader {
	public readonly string BaseDir;

	public ScriptLoader(string folder) {
		this.BaseDir = Path.TrimEndingDirectorySeparator(folder);
	}

	[Conditional("DEBUG")]
	private static void debug(string message)
		=> PluginLog.Information($"[LOADER] {message}");

	public string Clean(string dirty)
		=> Path.ChangeExtension(Path.Join(this.BaseDir, dirty), "lua");

	public bool IsPathUnderScriptRoot(string name) {
		string absolute = Path.GetFullPath(name);
		return Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(absolute) ?? Path.GetPathRoot(absolute) ?? string.Empty).StartsWith(this.BaseDir);
	}

	// I think this is for when you try to load a file by path, like with `loadfile`
	public string ResolveFileName(string filename, Table globalContext) {
		string absolute = Path.ChangeExtension(Path.Combine(this.BaseDir, filename), "lua");
		debug($"Resolving file '{filename}' to {absolute}");
		return absolute;
	}

	// And I think this is for when you try to load a module like with `require`
	public string ResolveModuleName(string modname, Table globalContext) {
		string absolute = Path.ChangeExtension(Path.Join(this.BaseDir, modname), "lua");
		debug($"Resolving module '{modname}' to {absolute}");
		return absolute;
	}

	// It looks like this needs to return a string consisting of the lua source to load
	public object LoadFile(string name, Table globalContext) {
		string absolute = Path.GetFullPath(name);
		debug($"Attempting to load {absolute}");

		if (!this.IsPathUnderScriptRoot(absolute))
			throw new ScriptRuntimeException($"Cannot load {absolute} (outside of module root {this.BaseDir})");

		if (!this.ScriptFileExists(absolute))
			return "return nil";

		debug($"Loading content from {absolute}");
		return File.ReadAllText(absolute);
	}

	// This one's pretty self-explanatory
	public bool ScriptFileExists(string name) {
		string absolute = Path.GetFullPath(name);
		debug($"Checking for existence/validity of {absolute}");
		return this.IsPathUnderScriptRoot(absolute) && File.Exists(Path.GetFullPath(absolute));
	}

}
