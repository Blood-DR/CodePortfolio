public static class GameSession
{
	/// <summary>
	/// Essentially used to track whether I start in game scene directly or 
	/// start from the main menu then select either new game or continue
	/// defaults endless mode as story writting is outside of functionality
	/// </summary>
	public static GameMode SelectedMode { get; set; } = GameMode.Endless;
	public static bool LaunchedFromMenu { get; set; } = false;
	public static bool IsContinue { get; set; } = false;
}
