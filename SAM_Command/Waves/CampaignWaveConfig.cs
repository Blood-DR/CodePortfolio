using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class CampaignWaveConfig : Component
{
	[Property] public List<DayWaveConfig> DayConfigs { get; set; } = new();

	public List<WaveEntry> GetEntriesForDay( int day )
	{
		var config = DayConfigs.FirstOrDefault( c => c.Day == day );
		return config != null
			? config.Entries.OrderBy( e => e.Delay ).ToList()
			: new List<WaveEntry>();
	}
}

public class DayWaveConfig
{
	[Property] public int Day { get; set; } = 1;
	[Property] public List<WaveEntry> Entries { get; set; } = new();
}
