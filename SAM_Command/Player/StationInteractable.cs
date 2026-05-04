using Sandbox;

/// Handler for "stations" and the player being able to "enter/interact" with them

public sealed class StationInteractable : Component
{
	[Property, Group( "Config" )] public string StationName { get; set; } = "Station";
	[Property, Group( "Config" )] public float InteractRange { get; set; } = 80f;


	/// Optional: if set, the camera moves to this position/rotation on enter and restores on exit.
	/// Leave null to keep the camera where it is.

	[Property, Group( "Config" )] public GameObject CameraPosition { get; set; }

	public bool PlayerInRange { get; private set; }

	protected override void OnUpdate()
	{
		if ( EscapeMenu.Instance?.IsOpen == true ) return;
		if ( StationManager.Instance == null ) return;

		var player = GameManager.Instance?.Player;
		if ( player == null ) return;

		var dist = (WorldPosition - player.WorldPosition).Length;
		PlayerInRange = dist <= InteractRange;

		// Already locked into this station — input handled by StationManager
		if ( StationManager.Instance.ActiveStation == this ) return;

		// manager cooldown so an exit press doesn't immediately re-enter
		if ( StationManager.Instance.LastStateChange < 0.25f ) return;

		if ( PlayerInRange && Input.Pressed( "Use" ) )
			StationManager.Instance.EnterStation( this );
	}

}
