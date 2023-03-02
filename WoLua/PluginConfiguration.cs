namespace PrincessRTFM.WoLua;

using Dalamud.Configuration;

internal class PluginConfiguration: IPluginConfiguration {
	public static class Defaults {
		public static string BasePath => @"C:\WoLua";

		public static bool RegisterDirectCommands = false;
	}

	public void Save()
		=> Service.Interface.SavePluginConfig(this);

	public int Version { get; set; } = 1;

	public string BasePath { get; set; } = Defaults.BasePath;

	public bool RegisterDirectCommands { get; set; } = Defaults.RegisterDirectCommands;
}
