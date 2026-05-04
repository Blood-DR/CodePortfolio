using Sandbox;


/// Homing missile launched by a SAMSite at a specific aircraft target.
/// Travels in a straight line toward the target's current world position,
/// rotates to face direction of travel, and destroys both itself and the
/// target on contact.
///
/// Self-destructs if the target is resolved (destroyed/leaked) before
/// impact, or if MaxLifetime expires (failsafe against runaway missiles).
///
/// Auto-registers with RadarSystem so it appears as a moving marker on the
/// radar scope.

public sealed class Missile : Component
{
	[Property, Group( "Flight" )] public float Speed { get; set; } = 800f;
	[Property, Group( "Flight" )] public float MaxLifetime { get; set; } = 30f;


	/// The aircraft this missile is chasing. Set by SAMSite.LaunchMissile()
	/// immediately after spawn.

	public Aircraft Target { get; set; }

	private TimeSince _spawnTime;

	protected override void OnStart()
	{
		_spawnTime = 0;
		RadarSystem.Instance?.RegisterMissile( GameObject );
	}

	protected override void OnDestroy()
	{
		RadarSystem.Instance?.UnregisterMissile( GameObject );
	}

	protected override void OnFixedUpdate()
	{
		// Failsafe — a missile that's been alive too long has likely gotten
		// stuck or its target was lost in a way we didn't catch.
		if ( _spawnTime > MaxLifetime )
		{
			GameObject.Destroy();
			return;
		}

		// Target was destroyed/leaked while we were in flight — fizzle out.
		if ( Target == null || Target.IsResolved )
		{
			GameObject.Destroy();
			return;
		}

		var toTarget  = Target.WorldPosition - WorldPosition;
		var distance  = toTarget.Length;
		var moveDelta = Speed * Time.Delta;

		// Will we cross the target this tick? Snap to it and detonate.
		if ( distance < moveDelta )
		{
			Target.MarkHit();
			GameObject.Destroy();
			return;
		}

		var direction = toTarget.Normal;
		WorldPosition += direction * moveDelta;
		WorldRotation  = Rotation.LookAt( direction );
	}
}
