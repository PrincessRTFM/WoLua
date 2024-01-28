namespace PrincessRTFM.WoLua.Constants;

public static class LogTag {
	public const string
		ObjectTable = "GAMEOBJ",
		FateTable = "FATE",
		GenerateDocs = "LUADOC",
		MethodTiming = "TIMER",
		DeprecatedApiMember = "DEPRECATION",
		CallbackRegistration = "CALLBACK",
		JsonParse = "JSON:PARSE",
		JsonDump = "JSON:SERIALISE",
		ActionQueue = "QUEUE",
		ActionCallback = $"{ActionQueue}:INVOKE",
		ActionPause = $"{ActionQueue}:PAUSE",
		DebugMessage = "DEBUG",
		ScriptInput = "INPUT",
		ScriptStorage = "STORAGE",
		LocalChat = "LOCALCHAT",
		ServerChat = "SERVERCHAT",
		Emotes = "EMOTE",
		Cleanup = "CLEAN",
		ScriptLoader = "LOADER",
		PluginCore = "CORE",
		Dispose = "DISPOSE";
}
