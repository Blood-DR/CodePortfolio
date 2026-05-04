namespace Sandbox;


/// Persistent player settings. Stored in FileSystem.OrganizationData (S&Box save location api)
/// so it's shared across all games in the org.

public class SettingsData
{
	public int MasterVolume { get; set; } = 50;
	public int MusicVolume { get; set; } = 50;
	public int SfxVolume { get; set; } = 50;
	public int MouseSensitivity { get; set; } = 50;
	public bool CrtEnabled { get; set; } = true;
	public bool Fullscreen { get; set; } = true;
	public int Version { get; set; } = 1;
}
