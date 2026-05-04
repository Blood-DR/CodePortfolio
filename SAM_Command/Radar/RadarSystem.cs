using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RadarSystem : Component
{
	public static RadarSystem Instance { get; private set; }

	[Property, Group( "Radar" )] public float SweepRPM { get; set; } = 5f;
	[Property, Group( "Radar" )] public float BlipDecayTime { get; set; } = 4f;
	[Property, Group( "Radar" )] public float RadarRange { get; set; } = 5000f;

	public float SweepAngle { get; private set; }
	public IReadOnlyList<RadarBlip> Blips => _blips;
	public IReadOnlyList<MissileMarker> Missiles => _missiles;
	public IReadOnlyList<SiteMarker> Sites => _sites;
	public IReadOnlyList<CityMarker> CityMarkers => _cityMarkers;
	public Aircraft SelectedContact { get; private set; }
	public SAMSite SelectedSite { get; private set; }
	public City SelectedCity { get; private set; }
	public FireSolution ActiveFireSolution { get; private set; }

	private readonly List<RadarBlip> _blips = new();
	private readonly List<MissileMarker> _missiles = new();
	private readonly List<SiteMarker> _sites = new();
	private readonly List<CityMarker> _cityMarkers = new();

	protected override void OnStart()
	{
		Instance = this;

		// Proactively register all cities and SAM sites already present in the
		// scene. This guards against initialization-order races: if City/SAMSite
		// OnStart fires before ours, RadarSystem.Instance is null at that point
		// and their self-registration silently no-ops. Scanning here catches them
		// regardless of order. RegisterCity/RegisterSite have dedup guards so
		// double-registration is harmless.
		foreach ( var city in Scene.GetAll<City>() )
			RegisterCity( city );
		foreach ( var site in Scene.GetAll<SAMSite>() )
			RegisterSite( site );
	}

	protected override void OnDestroy()
	{
		if ( Instance == this ) Instance = null;
	}

	protected override void OnUpdate()
	{
		TickSweep();
		TickMissiles();
		TickStaticMarkers();

		_blips.RemoveAll( b => b.Source.IsResolved || (float)b.PaintedAt > BlipDecayTime );

		if ( SelectedContact != null && SelectedContact.IsResolved )
		{
			SelectedContact = null;
			ActiveFireSolution = null;
		}

		if ( SelectedSite != null && (!SelectedSite.IsValid() || !SelectedSite.IsActive) )
			SelectedSite = null;

		if ( SelectedCity != null && (!SelectedCity.IsValid() || !SelectedCity.IsActive) )
			SelectedCity = null;

		if ( ActiveFireSolution != null && ActiveFireSolution.Target.CodeDirty )
			ActiveFireSolution = null;
	}

	void TickSweep()
	{
		var angleDelta = SweepRPM * 360f * Time.Delta / 60f;
		var prevAngle = SweepAngle;
		SweepAngle = (SweepAngle + angleDelta) % 360f;

		foreach ( var aircraft in Scene.GetAll<Aircraft>() )
		{
			if ( aircraft.IsResolved ) continue;

			var offset = aircraft.WorldPosition - WorldPosition;
			if ( offset.Length > RadarRange ) continue;

			var bearing = (MathF.Atan2( offset.x, offset.y ) * 180f / MathF.PI + 360f) % 360f;
			if ( !SweptThrough( bearing, prevAngle, SweepAngle ) ) continue;

			var panelPos = WorldOffsetToPanel( offset );
			var existing = _blips.FirstOrDefault( b => b.Source == aircraft );

			if ( existing != null )
			{
				existing.PanelPosition = panelPos;
				existing.WorldPositionAtPaint = aircraft.WorldPosition;
				existing.PaintedAt = 0;
			}
			else
			{
				_blips.Add( new RadarBlip
				{
					Source = aircraft,
					PanelPosition = panelPos,
					WorldPositionAtPaint = aircraft.WorldPosition,
					PaintedAt = 0
				} );
			}
		}
	}

	void TickMissiles()
	{
		_missiles.RemoveAll( m => !m.Source.IsValid() );
		foreach ( var marker in _missiles )
			marker.PanelPosition = WorldOffsetToPanel( marker.Source.WorldPosition - WorldPosition );
	}

	void TickStaticMarkers()
	{
		foreach ( var m in _sites )
			if ( m.Source.IsActive )
				m.PanelPosition = WorldOffsetToPanel( m.Source.WorldPosition - WorldPosition );
		foreach ( var m in _cityMarkers )
			if ( m.Source.IsActive )
				m.PanelPosition = WorldOffsetToPanel( m.Source.WorldPosition - WorldPosition );
	}

	public void RegisterSite( SAMSite site )
	{
		if ( _sites.Exists( m => m.Source == site ) ) return;
		_sites.Add( new SiteMarker { Source = site } );
	}

	public void UnregisterSite( SAMSite site ) => _sites.RemoveAll( m => m.Source == site );

	public void RegisterCity( City city )
	{
		if ( _cityMarkers.Exists( m => m.Source == city ) ) return;
		_cityMarkers.Add( new CityMarker { Source = city } );
	}

	public void UnregisterCity( City city ) => _cityMarkers.RemoveAll( m => m.Source == city );

	public void SelectContact( Aircraft aircraft )
	{
		SelectedContact = aircraft;
		SelectedSite = null;
		SelectedCity = null;
		ActiveFireSolution = null;

		// Treat fresh selection as a clean slate — the dirty flag tracks
		// "maneuvered since the player started watching", not "ever maneuvered".
		// Otherwise a contact that maneuvered before being selected gets a
		// stale "SOLUTION INVALID" warning the moment it's clicked.
		aircraft?.AcknowledgeDirty();
	}

	public void SelectSite( SAMSite site )
	{
		SelectedSite = site;
		SelectedContact = null;
		SelectedCity = null;
		ActiveFireSolution = null;
	}

	public void SelectCity( City city )
	{
		SelectedCity = city;
		SelectedContact = null;
		SelectedSite = null;
		ActiveFireSolution = null;
	}

	public FireSolution GenerateFireSolution()
	{
		if ( SelectedContact == null || SelectedContact.IsResolved ) return null;

		var blip = _blips.FirstOrDefault( b => b.Source == SelectedContact );
		if ( blip == null ) return null;

		return GenerateFireSolution( blip.WorldPositionAtPaint );
	}

	public FireSolution GenerateFireSolution( Vector3 paintedPosition )
	{
		if ( SelectedContact == null || SelectedContact.IsResolved ) return null;

		var offset = paintedPosition - WorldPosition;
		var bearing = (int)((MathF.Atan2( offset.x, offset.y ) * 180f / MathF.PI + 360f) % 360f);
		var range = (int)offset.Length;

		ActiveFireSolution = new FireSolution
		{
			ContactId = SelectedContact.GameObject.Name,
			Bearing = bearing,
			Range = range,
			Target = SelectedContact,
			FireControlCode = SelectedContact.FireControlCode
		};

		SelectedContact.AcknowledgeDirty();

		return ActiveFireSolution;
	}

	public void RegisterMissile( GameObject go )
	{
		_missiles.Add( new MissileMarker { Source = go } );
	}

	public void UnregisterMissile( GameObject go )
	{
		_missiles.RemoveAll( m => m.Source == go );
	}

	// +X = right (east), -X = left (west), +Y = top (north), -Y = bottom (south)
	Vector2 WorldOffsetToPanel( Vector3 offset ) => new(
		offset.x / RadarRange * 50f + 50f,
		-offset.y / RadarRange * 50f + 50f
	);

	static bool SweptThrough( float bearing, float prev, float curr )
	{
		if ( curr >= prev ) return bearing >= prev && bearing < curr;
		return bearing >= prev || bearing < curr;
	}
}

public class RadarBlip
{
	public Aircraft Source;
	public Vector2 PanelPosition;
	public Vector3 WorldPositionAtPaint;
	public TimeSince PaintedAt;
}

public class MissileMarker
{
	public GameObject Source;
	public Vector2 PanelPosition;
}

public class SiteMarker
{
	public SAMSite Source;
	public Vector2 PanelPosition;
}

public class CityMarker
{
	public City Source;
	public Vector2 PanelPosition;
}

public class FireSolution
{
	public string ContactId { get; set; }
	public int Bearing { get; set; }
	public int Range { get; set; }
	public Aircraft Target { get; set; }
	public string FireControlCode { get; set; }
}
