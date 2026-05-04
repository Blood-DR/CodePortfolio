using Sandbox;
using System;
using System.Linq;

public sealed class Aircraft : Component
{
	[Property, Group( "Contact" )] public ContactType Type { get; set; } = ContactType.Hostile;
	[Property, Group( "Movement" )] public float Speed { get; set; } = 200f;
	[Property, Group( "Maneuver" )] public float ManeuverChance { get; set; } = 0f;

	public City TargetCity { get; set; }
	public string FireControlCode { get; private set; }
	public bool CodeDirty { get; private set; }

	public bool HasLeaked { get; private set; }
	public bool IsDestroyed { get; private set; }
	public bool HasManeuvered { get; private set; }
	public bool IsResolved => HasLeaked || IsDestroyed;

	public static event Action<Aircraft> OnResolved;
	public static event Action<Aircraft> OnManeuver;

	private TimeUntil _nextManeuverCheck;

	protected override void OnStart()
	{
		FireControlCode = GenerateCode();
		HasManeuvered = false;
		ScheduleNextCheck();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsResolved || TargetCity == null ) return;

		var toTarget = TargetCity.WorldPosition - WorldPosition;
		if ( toTarget.Length < 50f )
		{
			Leak();
			return;
		}

		WorldPosition += toTarget.Normal * Speed * Time.Delta;

		if ( ManeuverChance > 0f && _nextManeuverCheck )
		{
			if ( Random.Shared.NextSingle() < ManeuverChance && !HasManeuvered)
				PerformManeuver();
			ScheduleNextCheck();
		}
	}

	public void AcknowledgeDirty() => CodeDirty = false;

	public void MarkHit()
	{
		if ( IsResolved ) return;
		IsDestroyed = true;
		OnResolved?.Invoke( this );
		GameObject.Destroy();
	}

	void Leak()
	{
		if ( IsResolved ) return;
		HasLeaked = true;
		TargetCity?.TakeDamage();
		OnResolved?.Invoke( this );
		GameObject.Destroy();
	}

	void PerformManeuver()
	{
		FireControlCode = GenerateCode();
		CodeDirty = true;
		HasManeuvered = true;
		var cities = Scene.GetAll<City>().Where( c => !c.IsDestroyed && c != TargetCity ).ToList();
		if ( cities.Count > 0 )
			TargetCity = cities[Random.Shared.Next( cities.Count )];

		OnManeuver?.Invoke( this );
	}

	void ScheduleNextCheck()
	{
		// Interval shrinks from ~10s to ~2s as ManeuverChance approaches 1
		_nextManeuverCheck = Random.Shared.NextSingle() * (8f - ManeuverChance * 6f) + 2f;
	}

	static string GenerateCode()
	{ 
		const string consonants = "BCDFGHJKLMNPRSTVY";
		const string digits = "23456789";
		var rng = Random.Shared;
		// example B2-G4 as a value that confirms what target we are targeting
		return $"{consonants[rng.Next( consonants.Length )]}{digits[rng.Next( digits.Length )]}-{consonants[rng.Next( consonants.Length )]}{digits[rng.Next( digits.Length )]}";
	}
}

public enum ContactType
{
	Hostile,
	Friendly,
	Spoofer,
	Decoy
}
