namespace PrincessRTFM.WoLua;

using Dalamud.Configuration;

internal class PluginConfiguration: IPluginConfiguration {
	public static class Defaults {
		public const string BasePath = @"C:\WoLua";
		public const string DirectInvocationCommandPrefix = "/";

		public const bool RegisterDirectCommands = false;
		public const bool PathNormalisation = false;
	}

	public void Save()
		=> Service.Interface.SavePluginConfig(this);

	public int Version { get; set; } = 1;

	public string BasePath { get; set; } = Defaults.BasePath;

	public string DirectInvocationCommandPrefix { get; set; } = Defaults.DirectInvocationCommandPrefix;

	public bool RegisterDirectCommands { get; set; } = Defaults.RegisterDirectCommands;

	public bool ExperimentalPathNormalisation { get; set; } = Defaults.PathNormalisation;
}
