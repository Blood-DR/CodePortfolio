using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class WaveSpawner : Component
{
	[Property] public GameObject AircraftPrefab { get; set; }
	[Property] public List<GameObject> SpawnPoints { get; set; } = new();


	/// First day on which endless waves take over (campaign covers 1..this-1).
	/// Used to normalize the endless difficulty curve so day N of endless
	/// always feels the same regardless of how long the campaign is.

	[Property] public int EndlessStartDay { get; set; } = 14;

	private List<WaveEntry> _pending = new();
	private readonly List<Aircraft> _active = new();
	private float _waveStartTime;
	private bool _waveActive;
	private int _nextEntry;

	public void BeginWave( int day )
	{
		var campaign = Components.GetInChildren<CampaignWaveConfig>( true );
		_pending = campaign != null
			? campaign.GetEntriesForDay( day )
			: GenerateEndlessEntries( day );

		_active.Clear();
		_nextEntry = 0;
		_waveStartTime = Time.Now;
		_waveActive = true;
		Aircraft.OnResolved += OnAircraftResolved;
	}

	protected override void OnUpdate()
	{
		if ( !_waveActive ) return;

		var elapsed = Time.Now - _waveStartTime;
		while ( _nextEntry < _pending.Count && _pending[_nextEntry].Delay <= elapsed )
		{
			SpawnEntry( _pending[_nextEntry] );
			_nextEntry++;
		}
	}

	void SpawnEntry( WaveEntry entry )
	{
		if ( AircraftPrefab == null || SpawnPoints.Count == 0 ) return;

		var cities = Scene.GetAll<City>().Where( c => c.IsActive && !c.IsDestroyed ).ToList();
		if ( cities.Count == 0 ) return;

		var rng = System.Random.Shared;
		var spawnPos = SpawnPoints[rng.Next( SpawnPoints.Count )].WorldPosition;
		var targetCity = cities[rng.Next( cities.Count )];

		for ( int i = 0; i < entry.Contacts.Count; i++ )
		{
			var cfg = entry.Contacts[i];
			var offset = entry.Formation == FormationType.Escort && i > 0
				? new Vector3( 0f, (i % 2 == 0 ? 1f : -1f) * 80f, 0f )
				: Vector3.Zero;

			var go = AircraftPrefab.Clone( spawnPos + offset );
			var aircraft = go.GetComponent<Aircraft>();
			if ( aircraft == null ) continue;

			aircraft.Type = cfg.Type;
			aircraft.Speed = cfg.Speed;
			aircraft.TargetCity = targetCity;
			aircraft.ManeuverChance = CalcManeuverChance( GameManager.Instance?.CurrentDay ?? 1 );
			_active.Add( aircraft );
		}
	}

	void OnAircraftResolved( Aircraft aircraft )
	{
		_active.Remove( aircraft );
		if ( _nextEntry >= _pending.Count && _active.Count == 0 )
			EndWave();
	}

	void EndWave()
	{
		_waveActive = false;
		Aircraft.OnResolved -= OnAircraftResolved;
		GameManager.Instance?.NotifyWaveComplete();
	}

	protected override void OnDestroy()
	{
		Aircraft.OnResolved -= OnAircraftResolved;
	}

	// 0% on day 1, ramps to ~60% by day 11+
	static float CalcManeuverChance( int day ) =>
		Math.Clamp( (day - 1) * 0.06f, 0f, 0.6f );

	// Endless wave generator — challenge scales by VOLUME, not speed.
	// Plane speed stays roughly constant (~120 with mild variance) so the
	// player's per-engagement reaction time is unchanged; instead the number
	// and density of contacts grows with the day, pressuring throughput.
	List<WaveEntry> GenerateEndlessEntries( int day )
	{
		var rng = System.Random.Shared;
		var entries = new List<WaveEntry>();

		const float BaseSpeed = 120f;

		// Normalize: the first endless day is "endless day 1" for scaling
		// purposes regardless of how long the campaign was. With
		// EndlessStartDay = 14, day 14 → endlessDay 1, day 28 → endlessDay 15.
		int endlessDay = Math.Max( 1, day - EndlessStartDay + 1 );

		// Group count grows linearly. Each "group" is one wave entry — solo or
		// an escort formation. More groups = more engagement events per wave.
		int groupCount = 2 + endlessDay / 2;

		// Escort probability and formation size both ramp with the day.
		// Late game sees big bomber formations rather than fast bombers.
		float escortChance  = Math.Clamp( 0.30f + endlessDay * 0.04f, 0.30f, 0.75f );
		int   maxEscortSize = Math.Clamp( 2 + endlessDay / 4, 2, 6 );

		// Inter-group spacing tightens slightly so threats start to overlap.
		float minDelay = Math.Max( 4f, 8f  - endlessDay * 0.10f );
		float maxDelay = Math.Max( 8f, 15f - endlessDay * 0.20f );

		float delay = 0f;
		for ( int i = 0; i < groupCount; i++ )
		{
			bool isEscort = endlessDay >= 2 && rng.NextSingle() < escortChance;
			int  count    = isEscort ? rng.Next( 2, maxEscortSize + 1 ) : 1;
			var  contacts = new List<ContactConfig>();

			for ( int j = 0; j < count; j++ )
			{
				// ±10% per-plane variance so a formation isn't pixel-perfect.
				var speed = BaseSpeed * (0.9f + rng.NextSingle() * 0.2f);
				contacts.Add( new ContactConfig { Type = ContactType.Hostile, Speed = speed } );
			}

			entries.Add( new WaveEntry
			{
				Delay     = delay,
				Formation = isEscort ? FormationType.Escort : FormationType.Solo,
				Contacts  = contacts,
			} );

			delay += minDelay + rng.NextSingle() * (maxDelay - minDelay);
		}

		return entries;
	}
}

public class WaveEntry
{
	[Property] public float Delay { get; set; } = 0f;
	[Property] public FormationType Formation { get; set; } = FormationType.Solo;
	[Property] public List<ContactConfig> Contacts { get; set; } = new();
}

public class ContactConfig
{
	[Property] public ContactType Type { get; set; } = ContactType.Hostile;
	[Property] public float Speed { get; set; } = 200f;
}

public enum FormationType
{
	Solo,
	Escort
}
