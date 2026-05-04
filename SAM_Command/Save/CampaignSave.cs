public class CampaignSave
{
	public int CurrentDay { get; set; } = 1;
	public int OperationsBudget { get; set; } = 0;
	public float PressureMeter { get; set; } = 0f;

	public List<UpgradeState> Upgrades { get; set; } = new();
	public List<CityState> Cities { get; set; } = new();
	public List<SAMSiteState> SAMSites { get; set; } = new();
	public List<DayConfig> DayHistory { get; set; } = new();

	public bool EndlessUnlocked { get; set; } = false;
	public bool IsComplete { get; set; } = false;
	public int SaveVersion { get; set; } = 1;
}

public class UpgradeState
{
	public string Id { get; set; }
	public int Tier { get; set; }
}

public class CityState
{
	public string Label { get; set; }
	public int CurrentHP { get; set; }
	public int MaxHP { get; set; }
}

public class SAMSiteState
{
	public string Id { get; set; }
	public int Ammo { get; set; }
	public bool IsDamaged { get; set; }
}
