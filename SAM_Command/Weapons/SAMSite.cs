using Sandbox;
using System.Linq;

public sealed class SAMSite : Component
{
	[Property, Group( "Config" )] public int ActiveFromDay { get; set; } = 1;
	[Property, Group( "Ammo" )] public int MaxAmmo { get; set; } = 10;
	[Property, Group( "Engagement" )] public float FiringArcDegrees { get; set; } = 120f;
	[Property, Group( "Engagement" )] public float EngagementRange { get; set; } = 3000f;

	// ── NPC autopilot ──────────────────────────────────────────────────────────
	// Sites that haven't been unlocked yet (IsActive == false) act as
	// autonomous allied defenders. They periodically pick a hostile in range
	// and arc, roll for fire, and launch. Once ActiveFromDay is reached and
	// the player takes control, this behaviour stops.
	// unsure if i'd keep this in the final game, but seemed like a cool little
	// feature.


	[Property, Group( "NPC AI" )] public float NpcFireChance { get; set; } = 0.01f;
	[Property, Group( "NPC AI" )] public float NpcCheckInterval { get; set; } = 6f;

	private TimeUntil _nextNpcCheck = 0f;


	/// Prefab cloned each time this site fires. Must contain a Missile component.

	[Property, Group( "Missile" )] public GameObject MissilePrefab { get; set; }

	/// Optional spawn point for missile (e.g. tip of launcher). Falls back to
	/// the SAMSite's own world position if not assigned.

	[Property, Group( "Missile" )] public GameObject LaunchPoint { get; set; }

	public int CurrentAmmo { get; private set; }
	public bool IsDamaged { get; private set; }
	public bool IsActive { get; set; } = false;
	public bool IsOperational => IsActive && !IsDamaged && CurrentAmmo > 0;

	protected override void OnStart()
	{
		CurrentAmmo = MaxAmmo;
		RadarSystem.Instance?.RegisterSite( this );

		// Stagger initial NPC checks across sites so a row of unlocked SAMs
		// doesn't all roll the dice on the same frame.
		_nextNpcCheck = System.Random.Shared.NextSingle() * NpcCheckInterval;
	}

	protected override void OnDestroy()
	{
		RadarSystem.Instance?.UnregisterSite( this );
	}

	protected override void OnUpdate()
	{
		// NPC autopilot — only when the site isn't player-controlled yet.
		if ( IsActive ) return;
		if ( IsDamaged ) return;
		if ( MissilePrefab == null ) return;
		if ( _nextNpcCheck > 0 ) return;

		_nextNpcCheck = NpcCheckInterval;
		TryNpcFire();
	}

	void TryNpcFire()
	{
		// Cheap check first — most ticks should bail out before scanning.
		if ( System.Random.Shared.NextSingle() > NpcFireChance ) return;

		var candidates = Scene.GetAll<Aircraft>()
			.Where( a => !a.IsResolved
				&& a.Type == ContactType.Hostile
				&& IsTargetEngageable( a ) )
			.ToList();

		if ( candidates.Count == 0 ) return;

		var target = candidates[System.Random.Shared.Next( candidates.Count )];
		LaunchMissile( target );
		Log.Info( $"[SAMSite] {GameObject.Name} (NPC) engaged {target.GameObject.Name}" );
	}

	bool IsTargetEngageable( Aircraft target )
	{
		var offset = target.WorldPosition - WorldPosition;
		if ( offset.Length > EngagementRange ) return false;

		// Project onto the horizontal plane and check the angle between the
		// site's forward and the direction to the target.
		var fwd = WorldRotation.Forward.WithZ( 0 );
		var dir = offset.WithZ( 0 );
		if ( fwd.LengthSquared < 0.001f || dir.LengthSquared < 0.001f )
			return true; // degenerate — fail open

		var dot   = Vector3.Dot( fwd.Normal, dir.Normal );
		var angle = System.MathF.Acos( System.Math.Clamp( dot, -1f, 1f ) ) * 180f / System.MathF.PI;

		return angle <= FiringArcDegrees / 2f;
	}

	public bool TryFire()
	{
		if ( !IsOperational ) return false;
		CurrentAmmo--;
		return true;
	}

	public EngagementResult TryFire( string enteredCode, Aircraft target )
	{
		if ( !IsOperational ) return EngagementResult.NotOperational;

		CurrentAmmo--;

		if ( !string.Equals( enteredCode, target.FireControlCode, System.StringComparison.OrdinalIgnoreCase ) )
			return EngagementResult.GuaranteedMiss;

		return EngagementResult.Valid;
	}

	public void Damage() => IsDamaged = true;
	public void Repair() => IsDamaged = false;
	public void Reload() => CurrentAmmo = MaxAmmo;


	/// Restore ammo from a saved value. Clamped to [0, MaxAmmo].

	public void SetAmmo( int ammo )
	{
		CurrentAmmo = System.Math.Clamp( ammo, 0, MaxAmmo );
	}


	/// Spawn a missile from this site that homes on the given target. No-op if
	/// MissilePrefab isn't assigned or target is null.

	public void LaunchMissile( Aircraft target )
	{
		if ( MissilePrefab == null || target == null ) return;

		var spawnPos = LaunchPoint != null ? LaunchPoint.WorldPosition : WorldPosition;
		var spawnRot = LaunchPoint != null ? LaunchPoint.WorldRotation : WorldRotation;

		var go = MissilePrefab.Clone( spawnPos, spawnRot );
		var missile = go.GetComponent<Missile>();
		if ( missile != null )
			missile.Target = target;
	}
}

public enum EngagementResult
{
	NotOperational,
	GuaranteedMiss,
	Valid
}
