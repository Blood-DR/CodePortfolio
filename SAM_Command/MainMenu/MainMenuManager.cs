using Sandbox;

// NOTE: most of the menu logic lives in MainMenu.razor's @code section.
// This component is kept for back-compat in case anything in the scene
// graph references its public methods directly.
public sealed class MainMenuManager : Component
{
	public bool CanContinue   => SaveSystem.HasSave() && !SaveSystem.IsCampaignComplete();
	public bool EndlessUnlocked => SaveSystem.IsEndlessUnlocked();

	public void OnNewGame()
	{
		SaveSystem.ResetCampaignProgress();
		GameSession.SelectedMode    = GameMode.Campaign;
		GameSession.LaunchedFromMenu = true;
		GameSession.IsContinue      = false;
		Scene.LoadFromFile( "Assests/scenes/maingame.scene" );
	}

	public void OnContinue()
	{
		if ( !CanContinue ) return;
		GameSession.SelectedMode    = GameMode.Campaign;
		GameSession.LaunchedFromMenu = true;
		GameSession.IsContinue      = true;
		Scene.LoadFromFile( "Assests/scenes/maingame.scene" );
	}

	public void OnEndlessMode()
	{
		if ( !EndlessUnlocked ) return;
		GameSession.SelectedMode    = GameMode.Endless;
		GameSession.LaunchedFromMenu = true;
		GameSession.IsContinue      = false;
		Scene.LoadFromFile( "Assests/scenes/maingame.scene" );
	}

	public void OnQuit()
	{
		Game.Close();
	}
}
