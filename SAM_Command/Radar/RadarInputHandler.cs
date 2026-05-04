using Sandbox;
using System;

public sealed class RadarInputHandler : Component
{
	public static RadarInputHandler Instance { get; private set; }

	[Property] public StationInteractable Station { get; set; }

	[Property, Group( "Radar Panel" )] public GameObject RadarPanelObject { get; set; }
	[Property, Group( "Radar Panel" )] public float RadarPanelWidth { get; set; } = 100f;
	[Property, Group( "Radar Panel" )] public float RadarPanelHeight { get; set; } = 100f;
	[Property, Group( "Radar Panel" )] public float BlipClickRadius { get; set; } = 6f;

	[Property, Group( "Contact Info Panel" )] public GameObject ContactInfoPanelObject { get; set; }
	[Property, Group( "Contact Info Panel" )] public float ContactInfoWidth { get; set; } = 100f;
	[Property, Group( "Contact Info Panel" )] public float ContactInfoHeight { get; set; } = 100f;

	// Top-left and bottom-right UV corners (0-100) for each button
	[Property, Group( "Contact Info Panel" )] public Vector2 SolutionButtonMin { get; set; } = new( 5f, 78f );
	[Property, Group( "Contact Info Panel" )] public Vector2 SolutionButtonMax { get; set; } = new( 95f, 89f );
	[Property, Group( "Contact Info Panel" )] public Vector2 ClearButtonMin { get; set; } = new( 5f, 93f );
	[Property, Group( "Contact Info Panel" )] public Vector2 ClearButtonMax { get; set; } = new( 95f, 100f );

	protected override void OnStart() => Instance = this;
	protected override void OnDestroy() { if ( Instance == this ) Instance = null; }

	protected override void OnUpdate()
	{
		if ( StationManager.Instance?.ActiveStation != Station ) return;
		if ( !Input.Pressed( "Attack1" ) ) return;
		HandleClick();
	}

	public void HandleClick()
	{
		if ( StationManager.Instance?.ActiveStation != Station ) return;
		if ( RadarSystem.Instance == null ) return;

		var camera = Scene.Camera;
		if ( camera == null ) return;

		var ray = camera.ScreenPixelToRay( Mouse.Position );
		var tr = Scene.Trace.Ray( ray, 2000f ).Run();
		if ( !tr.Hit ) return;

		if ( tr.GameObject == RadarPanelObject )
		{
			var panelPos = HitToPanel( tr.HitPosition, RadarPanelObject, RadarPanelWidth, RadarPanelHeight );
			TrySelectBlip( panelPos );
		}
		else if ( tr.GameObject == ContactInfoPanelObject )
		{
			var panelPos = HitToPanel( tr.HitPosition, ContactInfoPanelObject, ContactInfoWidth, ContactInfoHeight );
			HandleContactInfoClick( panelPos );
		}
	}

	Vector2 HitToPanel( Vector3 worldHit, GameObject panel, float width, float height )
	{
		var offset = worldHit - panel.WorldPosition;
		var local = panel.WorldRotation.Inverse * offset;

		// local Y = horizontal on panel (positive = right)
		// local Z = vertical on panel (positive = up)
		var u = (local.y / width + 0.5f) * 100f;
		var v = (-local.z / height + 0.5f) * 100f;

		return new Vector2( u, v );
	}

	static bool InRect( Vector2 pos, Vector2 min, Vector2 max ) =>
		pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y;

	void TrySelectBlip( Vector2 panelPos )
	{
		Log.Info( $"[RadarInputHandler] RadarPanel hit at UV ({panelPos.x:F1}, {panelPos.y:F1})" );

		var radar = RadarSystem.Instance;
		float nearestDist = BlipClickRadius;
		object nearest = null; // RadarBlip | SiteMarker | CityMarker

		foreach ( var blip in radar.Blips )
		{
			var dist = (blip.PanelPosition - panelPos).Length;
			if ( dist < nearestDist ) { nearestDist = dist; nearest = blip; }
		}

		foreach ( var site in radar.Sites )
		{
			var dist = (site.PanelPosition - panelPos).Length;
			if ( dist < nearestDist ) { nearestDist = dist; nearest = site; }
		}

		foreach ( var city in radar.CityMarkers )
		{
			var dist = (city.PanelPosition - panelPos).Length;
			if ( dist < nearestDist ) { nearestDist = dist; nearest = city; }
		}

		if ( nearest is RadarBlip b )
			radar.SelectContact( b.Source );
		else if ( nearest is SiteMarker s )
			radar.SelectSite( s.Source );
		else if ( nearest is CityMarker c )
			radar.SelectCity( c.Source );
		else
			radar.SelectContact( null ); // click on empty space — deselect all
	}

	void HandleContactInfoClick( Vector2 panelPos )
	{
		Log.Info( $"[RadarInputHandler] ContactInfoPanel hit at UV ({panelPos.x:F1}, {panelPos.y:F1})" );

		if ( RadarSystem.Instance.SelectedContact == null ) return;

		if ( InRect( panelPos, SolutionButtonMin, SolutionButtonMax ) )
		{
			ContactInfoPanel.Instance?.RequestSolution();
		}
		else if ( InRect( panelPos, ClearButtonMin, ClearButtonMax ) )
		{
			Log.Info( $"[RadarInputHandler] CLEAR clicked" );
			RadarSystem.Instance.SelectContact( null );
		}
		else
		{
			Log.Info( $"[RadarInputHandler] No button matched — SolutionBtn ({SolutionButtonMin})-({SolutionButtonMax}), ClearBtn ({ClearButtonMin})-({ClearButtonMax})" );
		}
	}
}
