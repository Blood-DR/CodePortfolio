using Sandbox;

public sealed class City : Component
{
	//In engine editable fields per city with default values
	[Property, Group( "Config" )] public string Label { get; set; } = "City";
	[Property, Group( "Config" )] public int ActiveFromDay { get; set; } = 1;
	[Property, Group( "HP" )] public int MaxHP { get; set; } = 3;

	public int CurrentHP { get; private set; }
	public bool IsActive { get; set; } = false;
	public bool IsDestroyed => CurrentHP <= 0;

	public delegate void CityDamaged( City city, int remaining );
	public static event CityDamaged OnDamaged;

	public delegate void CityDestroyed( City city );
	public static event CityDestroyed OnDestroyed;

	protected override void OnStart()
	{
		//Cities self register with the Radar so we can add/remove them without needing changes on the radar side
		CurrentHP = MaxHP;
		RadarSystem.Instance?.RegisterCity( this );
	}

	protected override void OnDestroy()
	{
		RadarSystem.Instance?.UnregisterCity( this );
	}

	/// <summary>
	/// Restore HP from a saved value. Clamped to [0, MaxHP] in case MaxHP changed
	/// between save versions.
	/// </summary>
	public void SetHP( int hp )
	{
		CurrentHP = System.Math.Clamp( hp, 0, MaxHP );
	}

	public void TakeDamage( int amount = 1 )
	{
		if ( !IsActive || IsDestroyed ) return;
		CurrentHP = System.Math.Max( 0, CurrentHP - amount );
		OnDamaged?.Invoke( this, CurrentHP );
		Log.Info( $"[City] {Label} hit — {CurrentHP}/{MaxHP} HP remaining" );

		if ( IsDestroyed )
			OnDestroyed?.Invoke( this );
	}
}
