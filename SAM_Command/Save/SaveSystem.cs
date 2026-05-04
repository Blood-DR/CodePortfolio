public static class SaveSystem
{
	private const string CampaignFile = "campaign.json";

	public static void Save( CampaignSave data )
	{
		FileSystem.Data.WriteJson( CampaignFile, data );
	}

	public static CampaignSave Load()
	{
		if ( !FileSystem.Data.FileExists( CampaignFile ) )
			return new CampaignSave(); // fresh run

		return FileSystem.Data.ReadJson<CampaignSave>( CampaignFile );
	}

	public static bool HasSave() => FileSystem.Data.FileExists( CampaignFile );

	public static bool IsCampaignComplete()
	{
		if ( !HasSave() ) return false;
		return Load().IsComplete;
	}

	public static bool IsEndlessUnlocked()
	{
		if ( !HasSave() ) return false;
		return Load().EndlessUnlocked;
	}

	public static void DeleteSave()
	{
		if ( FileSystem.Data.FileExists( CampaignFile ) )
			FileSystem.Data.DeleteFile( CampaignFile );
	}


	/// Reset the campaign run-state but preserve meta flags (EndlessUnlocked
	/// in particular). Used by the New Game button — the player gets a fresh
	/// campaign without losing their endless-mode unlock from a prior run.

	public static void ResetCampaignProgress()
	{
		// If there's no save yet, nothing to preserve — just exit and let
		// the GameManager start a fresh run normally.
		if ( !HasSave() ) return;

		var existing = Load();

		var fresh = new CampaignSave
		{
			// Preserve meta flags
			EndlessUnlocked = existing.EndlessUnlocked,
			SaveVersion    = existing.SaveVersion,

			// Reset everything campaign-related (defaults from the model)
			CurrentDay       = 1,
			OperationsBudget = 0,
			PressureMeter    = 0f,
			Upgrades         = new(),
			Cities           = new(),
			SAMSites         = new(),
			DayHistory       = new(),
			IsComplete       = false,
		};

		Save( fresh );
	}
}
