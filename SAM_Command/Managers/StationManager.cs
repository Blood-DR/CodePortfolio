using Sandbox;
using System.Reflection;


/// Singleton that tracks which bunker station the player is currently locked into.
/// Frees/restores mouse and player look input on enter/exit.

public sealed class StationManager : Component
{
	public static StationManager Instance { get; private set; }


	/// Assign the component that handles camera look (yaw/pitch).
	/// It will be disabled on station enter and re-enabled on exit.

	[Property] public Component LookController { get; set; }

	public StationInteractable ActiveStation { get; private set; }
	public bool IsLockedIn => ActiveStation != null;

	// ── Saved transforms (WORLD space) ─────────────────────────────────────────
	private Vector3  _savedPlayerPosition;
	private Rotation _savedPlayerRotation;
	private Vector3  _savedCamWorldPosition;
	private Rotation _savedCamWorldRotation;

	// LookController's internal yaw/pitch state, captured via reflection.
	// Most FPS look controllers store an Angles field/property they integrate
	// each frame; if we don't restore it, disabling+re-enabling resets it to
	// zero and the camera snaps to (0,0,0) on station exit.
	private Angles? _savedLookAngles;

	// Multi-frame restore window — fallback if reflection couldn't find the
	// LookController's internal angles. Forces world transforms each frame.
	private int _restoreFramesLeft = 0;
	private const int RestoreFrameCount = 30;

	/// Time since the station state last changed (enter or exit).
	public TimeSince LastStateChange { get; private set; } = 1f;

	public delegate void StationChanged( StationInteractable station );
	public static event StationChanged OnStationEntered;
	public static event StationChanged OnStationExited;

	// Common names for the angles field/property across S&Box look controllers.
	// Tried in order; first match wins for both get and set.
	static readonly string[] AngleMemberNames =
	{
		"EyeAngles", "ViewAngles", "AimAngles", "LookAngles", "Angles", "CameraAngles",
		"eyeAngles", "viewAngles", "aimAngles", "lookAngles", "angles", "cameraAngles",
		"_eyeAngles", "_viewAngles", "_angles",
	};

	protected override void OnStart() { Instance = this; }

	protected override void OnDestroy()
	{
		if ( Instance == this ) Instance = null;
	}

	protected override void OnUpdate()
	{
		// Belt-and-suspenders: even after the explicit restore in ExitStation,
		// hold the saved transforms + look angles for several frames in case
		// LookController fights back during its first frames after re-enable.
		if ( _restoreFramesLeft > 0 )
		{
			_restoreFramesLeft--;
			ForceSavedTransforms();
			if ( _savedLookAngles.HasValue )
				TrySetLookAngles( _savedLookAngles.Value );
		}

		if ( EscapeMenu.Instance?.IsOpen == true ) return;
		if ( !IsLockedIn ) return;
		if ( LastStateChange < 0.25f ) return;
		if ( Terminal.IsCapturingKeyboard ) return;

		if ( Input.Pressed( "Use" ) )
			ExitStation();
	}

	void ForceSavedTransforms()
	{
		var player = GameManager.Instance?.Player;
		if ( player != null )
		{
			player.WorldPosition = _savedPlayerPosition;
			player.WorldRotation = _savedPlayerRotation;
		}

		var cam = Scene.Camera?.GameObject;
		if ( cam != null )
		{
			cam.WorldPosition = _savedCamWorldPosition;
			cam.WorldRotation = _savedCamWorldRotation;
		}
	}

	public void EnterStation( StationInteractable station )
	{
		if ( IsLockedIn ) ExitStation();

		ActiveStation   = station;
		LastStateChange = 0;
		Mouse.Visibility = MouseVisibility.Visible;

		// Save world transforms BEFORE disabling anything.
		var player = GameManager.Instance?.Player;
		if ( player != null )
		{
			_savedPlayerPosition = player.WorldPosition;
			_savedPlayerRotation = player.WorldRotation;
		}

		var cam = Scene.Camera?.GameObject;
		if ( cam != null )
		{
			_savedCamWorldPosition = cam.WorldPosition;
			_savedCamWorldRotation = cam.WorldRotation;
		}

		// Capture LookController's internal angles state via reflection.
		_savedLookAngles = TryGetLookAngles();

		Log.Info( $"[StationManager] ENTER — saved player @ {_savedPlayerPosition} / {_savedPlayerRotation.Angles()}" );
		Log.Info( $"[StationManager] ENTER — saved cam    @ {_savedCamWorldPosition} / {_savedCamWorldRotation.Angles()}" );
		Log.Info( $"[StationManager] ENTER — saved look angles: {(_savedLookAngles?.ToString() ?? "<not found>")}" );

		// Now disable controllers + player.
		if ( LookController != null ) LookController.Enabled = false;

		if ( player != null )
		{
			var cc = player.Components.Get<CharacterController>();
			if ( cc != null ) cc.Velocity = Vector3.Zero;
			player.Enabled = false;
		}

		// Move camera to the station view.
		if ( cam != null && station.CameraPosition != null )
		{
			cam.WorldPosition = station.CameraPosition.WorldPosition;
			cam.WorldRotation = station.CameraPosition.WorldRotation;
		}

		OnStationEntered?.Invoke( station );
	}

	public void ExitStation()
	{
		if ( !IsLockedIn ) return;

		var previous = ActiveStation;
		ActiveStation   = null;
		LastStateChange = 0;
		Mouse.Visibility = MouseVisibility.Hidden;

		// 1. Restore player BEFORE re-enabling.
		var player = GameManager.Instance?.Player;
		if ( player != null )
		{
			player.WorldPosition = _savedPlayerPosition;
			player.WorldRotation = _savedPlayerRotation;
			player.Enabled = true;
		}

		// 2. Restore camera world transform.
		var cam = Scene.Camera?.GameObject;
		if ( cam != null )
		{
			cam.WorldPosition = _savedCamWorldPosition;
			cam.WorldRotation = _savedCamWorldRotation;
		}

		// 3. CRITICAL: restore LookController's internal angles BEFORE re-enabling
		//    so that when its OnEnabled / first OnUpdate fires, it computes
		//    rotation from the correct yaw/pitch instead of zero.
		if ( _savedLookAngles.HasValue )
			TrySetLookAngles( _savedLookAngles.Value );

		if ( LookController != null ) LookController.Enabled = true;

		// 4. And one more time AFTER enable, in case OnEnabled reset state.
		if ( _savedLookAngles.HasValue )
			TrySetLookAngles( _savedLookAngles.Value );

		Log.Info( $"[StationManager] EXIT — restored look angles: {(_savedLookAngles?.ToString() ?? "<none saved>")}" );

		// 5. Hold the restoration for several frames as a final safety net.
		_restoreFramesLeft = RestoreFrameCount;

		OnStationExited?.Invoke( previous );
	}

	// ── Reflection helpers ─────────────────────────────────────────────────────

	Angles? TryGetLookAngles()
	{
		if ( LookController == null ) return null;
		var type = LookController.GetType();

		foreach ( var name in AngleMemberNames )
		{
			var prop = type.GetProperty( name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			if ( prop != null && prop.CanRead && prop.PropertyType == typeof( Angles ) )
			{
				Log.Info( $"[StationManager] Found LookController angles via property: {type.Name}.{name}" );
				return (Angles)prop.GetValue( LookController );
			}

			var field = type.GetField( name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			if ( field != null && field.FieldType == typeof( Angles ) )
			{
				Log.Info( $"[StationManager] Found LookController angles via field: {type.Name}.{name}" );
				return (Angles)field.GetValue( LookController );
			}
		}

		Log.Warning( $"[StationManager] No Angles member found on {type.Name}. Add its field/property name to AngleMemberNames." );
		return null;
	}

	void TrySetLookAngles( Angles angles )
	{
		if ( LookController == null ) return;
		var type = LookController.GetType();

		foreach ( var name in AngleMemberNames )
		{
			var prop = type.GetProperty( name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			if ( prop != null && prop.CanWrite && prop.PropertyType == typeof( Angles ) )
			{
				prop.SetValue( LookController, angles );
				return;
			}

			var field = type.GetField( name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			if ( field != null && field.FieldType == typeof( Angles ) )
			{
				field.SetValue( LookController, angles );
				return;
			}
		}
	}
}
