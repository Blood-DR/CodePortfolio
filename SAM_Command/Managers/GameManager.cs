using Sandbox;
using System.Linq;
using System.Text.Json.Serialization;

public sealed class GameManager : Component
{
	public static GameManager Instance { get; private set; }

	[Property, Group( "Day Config" )] public int CurrentDay { get; private set; } = 1;
	[Property, Group( "Day Config" )] public float WarmUpDuration { get; set; } = 30f;
	[Property, Group( "Day Config" )] public float Money { get; private set; } = 0f;

	[Property, Group( "Pressure" )] public float PressureMeter { get; private set; } = 0f;
	[Property, Group( "Pressure" )] public float MaxPressure { get; set; } = 100f;

[Property, Group( "Player" )] public GameObject Player { get; set; }
	[Property, Group( "Player" )] public Vector3 StartLocation { get; set; }

	[Property, Group( "Game Config" )] public bool InTutorial { get; set; } = true;
	[Property, Group( "Game Config" )] public GameMode Mode { get; set; } = GameMode.Endless;

	[Property, Group( "References" )] public WaveSpawner WaveSpawner { get; set; }

	public DayConfig CurrentDaySession { get; private set; }
	public DayPhase Phase { get; private set; } = DayPhase.WarmUp;
	public float WarmUpTimeRemaining { get; private set; }

	public delegate void DayPhaseChanged( DayPhase phase );
	public event DayPhaseChanged OnPhaseChanged;

	// Save data deferred to first OnUpdate. We can't apply city HP / SAM ammo
	// in OnStart because Scene component init order isn't guaranteed — those
	// components might not have OnStart-ed yet (which would reset the values
	// we just restored). Applying one frame later avoids that race.
	private CampaignSave _pendingSaveApply;

	// Checkpoint save deferred to next OnUpdate for the same reason: City.CurrentHP
	// and SAMSite.CurrentAmmo default to 0 and are only set to MaxHP/MaxAmmo in
	// their own OnStart, which may not have run yet when BeginDay is called from
	// GameManager.OnStart. One frame later everything is initialized correctly.
	private bool _pendingStartOfDaySave;

	protected override void OnStart()
	{
		Instance = this;
		if ( GameSession.LaunchedFromMenu )
			Mode = GameSession.SelectedMode;

		// Continue: explicit Continue button clicked AND a save exists. Load
		// everything from disk and resume.
		if ( GameSession.IsContinue && SaveSystem.HasSave() )
		{
			var save = SaveSystem.Load();
			CurrentDay = save.CurrentDay;
			Money = save.OperationsBudget;
			InTutorial = false;
			_pendingSaveApply = save;
			Log.Info( $"[GameManager] Continue — loaded save (day {CurrentDay}, ${Money})" );
			BeginDay();
		}
		else if ( Mode == GameMode.Endless )
		{
			// Endless button: fresh endless run, ignore saved campaign progress.
			// Start at the configured endless-start day so wave generation kicks in.
			CurrentDay = WaveSpawner?.EndlessStartDay ?? 14;
			InTutorial = false;
			BeginDay();
		}
		else
		{
			// New Game (campaign): the menu has already reset campaign progress
			// in the save (preserving meta flags). Start day 0.
			// starts in tutorial again
			InTutorial = true;
			CurrentDay = 0;
			StartTutorial();
		}
	}


	protected override void OnDestroy()
	{
		if ( Instance == this ) Instance = null;
	}

	protected override void OnUpdate()
	{
		// Apply restored world state on the first frame after OnStart, by
		// which time all City and SAMSite components have run their own
		// OnStart and reset their fields to defaults. Our restore overrides
		// those defaults.
		if ( _pendingSaveApply != null )
		{
			ApplySaveStateToWorld( _pendingSaveApply );
			_pendingSaveApply = null;
		}

		// Write the start-of-day checkpoint one frame after BeginDay so that
		// City.CurrentHP and SAMSite.CurrentAmmo have been initialised by their
		// own OnStart calls (they default to 0 before that runs).
		if ( _pendingStartOfDaySave )
		{
			SaveProgress( addToHistory: false );
			_pendingStartOfDaySave = false;
		}

		if ( Phase == DayPhase.WarmUp )
			TickWarmUp();
	}

	void ApplySaveStateToWorld( CampaignSave save )
	{
		// Cities — restore HP by Label match. Cities not present in save (e.g.
		// added later via ActiveFromDay) keep their default full HP.
		foreach ( var cityState in save.Cities )
		{
			var city = Scene.GetAll<City>()
				.FirstOrDefault( c => c.Label == cityState.Label );
			if ( city != null )
				city.SetHP( cityState.CurrentHP );
		}

		// SAM sites — restore ammo + damage state by GameObject name.
		foreach ( var siteState in save.SAMSites )
		{
			var site = Scene.GetAll<SAMSite>()
				.FirstOrDefault( s => s.GameObject.Name == siteState.Id );
			if ( site == null ) continue;

			site.SetAmmo( siteState.Ammo );
			if ( siteState.IsDamaged ) site.Damage();
			else                       site.Repair();
		}

		Log.Info( $"[GameManager] Restored {save.Cities.Count} cities, {save.SAMSites.Count} SAM sites from save" );
	}

	void TickWarmUp()
	{
		WarmUpTimeRemaining -= Time.Delta;
		if ( WarmUpTimeRemaining <= 0f && !InTutorial)
		{
			WarmUpTimeRemaining = 0f;
			SetPhase( DayPhase.Wave );
			WaveSpawner?.BeginWave( CurrentDay );
		}
	}

	void BeginDay()
	{
		CurrentDaySession   = new DayConfig { Day = CurrentDay };
		PressureMeter       = 0f;
		WarmUpTimeRemaining = WarmUpDuration;
		ActivateDayEntities();
		SetPlayerToSpawn( StartLocation, Player );
		SetPhase( DayPhase.WarmUp );

		// Queue a checkpoint save for next OnUpdate. Deferred so City.CurrentHP and
		// SAMSite.CurrentAmmo are populated by the time we write (they default to 0
		// and are only set in each component's OnStart, which may not have fired yet
		// if BeginDay is called during GameManager.OnStart).
		// Guards:
		//   _pendingSaveApply != null  → continue-load in progress; existing save
		//     is already correct, don't overwrite with un-restored defaults.
		//   Mode == Endless            → endless runs don't use the campaign save.
		if ( _pendingSaveApply == null && Mode == GameMode.Campaign )
			_pendingStartOfDaySave = true;
	}

	void ActivateDayEntities()
	{
		foreach ( var site in Scene.GetAll<SAMSite>() )
			site.IsActive = site.ActiveFromDay <= CurrentDay;

		foreach ( var city in Scene.GetAll<City>() )
			city.IsActive = city.ActiveFromDay <= CurrentDay;
	}

	public void NotifyWaveComplete()
	{
		if ( Phase != DayPhase.Wave ) return;
		SetPhase( DayPhase.Debrief );
	}

	public void ConfirmDebrief()
	{
		if ( Phase != DayPhase.Debrief ) return;
		SetPhase( DayPhase.Save );
		CurrentDay++;

		// Detect end-of-campaign: campaign covers days 1..(EndlessStartDay - 1).
		// Once CurrentDay reaches EndlessStartDay the player has finished the
		// final campaign day.
		int endlessStart = WaveSpawner?.EndlessStartDay ?? 14;
		bool justFinishedCampaign = Mode == GameMode.Campaign && CurrentDay >= endlessStart;

		if ( justFinishedCampaign )
		{
			// Must save before changing scenes — this is the only place where
			// IsComplete and EndlessUnlocked get written.
			SaveProgress( markComplete: true );
			Log.Info( "[GameManager] Campaign complete — returning to main menu" );
			ReturnToMainMenu();
			return;
		}

		// Normal debrief: advance to the next day. BeginDay writes the
		// start-of-day checkpoint so Continue always resumes here.
		BeginDay();
	}

	void SaveProgress( bool markComplete = false, bool addToHistory = true )
	{
		var save = SaveSystem.HasSave() ? SaveSystem.Load() : new CampaignSave();

		save.CurrentDay       = CurrentDay;
		save.OperationsBudget = (int)Money;
		save.PressureMeter    = PressureMeter;

		// Only append to history when the day has actually finished (e.g.
		// campaign-complete debrief). Start-of-day checkpoints pass
		// addToHistory: false to avoid storing a blank, not-yet-played session.
		if ( addToHistory )
			save.DayHistory.Add( CurrentDaySession );

		save.Cities = Scene.GetAll<City>().Select( c => new CityState
		{
			Label = c.Label,
			CurrentHP = c.CurrentHP,
			MaxHP = c.MaxHP
		} ).ToList();

		save.SAMSites = Scene.GetAll<SAMSite>().Select( s => new SAMSiteState
		{
			Id = s.GameObject.Name,
			Ammo = s.CurrentAmmo,
			IsDamaged = s.IsDamaged
		} ).ToList();

		if ( markComplete )
		{
			save.IsComplete      = true;
			save.EndlessUnlocked = true;
		}

		SaveSystem.Save( save );
		Log.Info( $"[GameManager] Saved — Day {CurrentDay}{(markComplete ? " [COMPLETE]" : "")}" );
	}

	void ReturnToMainMenu()
	{
		Scene.TimeScale       = 1f;
		Mouse.Visibility      = MouseVisibility.Visible;
		var options = new SceneLoadOptions { ShowLoadingScreen = false };
		options.SetScene( "scenes/mainmenu.scene" );
		Sandbox.Game.ChangeScene( options );
	}

	void SetPhase( DayPhase phase )
	{
		Phase = phase;
		OnPhaseChanged?.Invoke( phase );
		Log.Info( $"[GameManager] Phase -> {phase} (Day {CurrentDay})" );
	}

	void SetPlayerToSpawn( Vector3 pos, GameObject playerObj )
	{
		if ( playerObj != null )
			playerObj.WorldPosition = pos;
	}

	void StartTutorial()
	{
		Log.Info( "[GameManager] -> Entering Tutorial" );
		SetPlayerToSpawn( StartLocation, Player );
		SetPhase( DayPhase.WarmUp );
		//tutorial dialog / tutorial stuff
		

		EndTutorial();
	}

	void EndTutorial()
	{
		Log.Info( "[GameManager] -> Finishing Tutorial" );
		InTutorial = false;
		CurrentDay++;
		BeginDay();
	}
}

public enum DayPhase
{
	WarmUp,
	Wave,
	Debrief,
	Save
}

public enum GameMode
{
	Endless,
	Campaign
}

public class DayConfig
{
	[JsonPropertyName( "day" )] public int Day { get; set; } = 0;
	[JsonPropertyName( "moneyEarned" )] public float MoneyEarned { get; set; } = 0f;
	[JsonPropertyName( "friendlyKills" )] public int FriendlyKills { get; set; } = 0;
	[JsonPropertyName( "friendlyDamaged" )] public bool FriendlyDamaged { get; set; } = false;
	[JsonPropertyName( "kills" )] public int Kills { get; set; } = 0;
	[JsonPropertyName( "leakers" )] public int Leakers { get; set; } = 0;
}

