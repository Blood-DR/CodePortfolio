namespace Sandbox;

public static class SettingsSystem
{
	private const string SettingsFile = "settings.json";

	public static void Save( SettingsData data )
	{
		FileSystem.OrganizationData.WriteJson( SettingsFile, data );
	}

	public static SettingsData Load()
	{
		if ( !FileSystem.OrganizationData.FileExists( SettingsFile ) )
			return new SettingsData();

		return FileSystem.OrganizationData.ReadJson<SettingsData>( SettingsFile );
	}

	public static bool HasSettings() => FileSystem.OrganizationData.FileExists( SettingsFile );

	public static void DeleteSettings()
	{
		if ( FileSystem.OrganizationData.FileExists( SettingsFile ) )
			FileSystem.OrganizationData.DeleteFile( SettingsFile );
	}
}
