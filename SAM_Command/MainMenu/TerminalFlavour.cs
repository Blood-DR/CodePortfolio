namespace Sandbox;
public static class TerminalFlavour
{
	// === First-boot intro sequence ===
	public class BootCommand
	{
		public string Command { get; set; }
		public string[] Output { get; set; }
	}

	// Commands "typed" by the operator BEFORE the audio calibration prompt
	public static readonly BootCommand[] BootCommandsPre = new[]
	{
		new BootCommand
		{
			Command = "system --boot",
			Output = new[]
			{
				"SAM COMMAND TERMINAL v2.3",
				"(c) STATE BUREAU OF DEFENCE",
				"",
			},
		},

		new BootCommand
		{
			Command = "storage --mount",
			Output = new[]
			{
				"[ STRG  ] mounting volumes ..... ok",
				"[ STRG  ] /BOOT ................ ok",
				"[ STRG  ] /SYS ................. ok",
				"[ STRG  ] /SAM ................. ok",
				"[ STRG  ] /COMMS ............... ok",
				"[ STRG  ] /ARCHIVE ............. degraded",
				"[ STRG  ] /PEACEAGREEMENT ...... missing",
				"",
			},
		},

		new BootCommand
		{
			Command = "diagnostics --memory",
			Output = new[]
			{
				"[ MEM   ] bank 01 (64k) ........ ok",
				"[ MEM   ] bank 02 (64k) ........ ok",
				"[ MEM   ] parity check ......... ok",
				"",
			},
		},
		new BootCommand
		{
			Command = "network --uplink",
			Output = new[]
			{
				"[ NET   ] uplink card detected . ok",
				"[ NET   ] handshake ............ ok",
				"[ NET   ] link to command ...... established",
				"",
			},
		},
		new BootCommand
		{
			Command = "auth operator_07",
			Output = new[]
			{
				"[ AUTH  ] biometric scan ....... ok",
				"[ AUTH  ] credentials .......... verified",
				"",
			},
		},
		new BootCommand
		{
			Command = "systems --check-all",
			Output = new[]
			{
				"[ SAM   ] fire control array ... online",
				"[ SAM   ] all sites ............ green",
				"[ IFF   ] decoder mk2 .......... online",
				"[ RDR   ] dome motor ........... spinning up",
				"[ RDR   ] sweep cycle .......... stable",
				"[ ENV   ] bunker seal verified . ok",
				"[ COMMS ] encryption loaded .... ok",
				"",
			},
		},
		new BootCommand
		{
			Command = "audio --calibrate",
			Output = new[]
			{
				"[ AUDIO ] output muted",
				"[ AUDIO ] calibration required",
				"",
			},
		},
	};

	// Commands "typed" by the operator AFTER the audio calibration completes
	public static readonly BootCommand[] BootCommandsPost = new[]
	{
		new BootCommand
		{
			Command = "audio --confirm",
			Output = new[]
			{
				"[ AUDIO ] levels confirmed",
				"",
			},
		},
		new BootCommand
		{
			Command = "launch operator_terminal",
			Output = new[]
			{
				"[ SYS   ] boot sequence complete",
				"[ SYS   ] loading operator terminal...",
				"",
			},
		},
	};


	public static readonly string[] Subtitles = new[]
	{
		"// OPERATOR TERMINAL v1.0",
		"// BOOT SEQUENCE: COMPLETE",
		"// SAM ARRAY: NOMINAL",
		"// IFF CHANNEL: SECURE",
		"// AWAITING COMMAND",
		"// PHOSPHOR ALIGNMENT: STABLE",
		"// STRATEGIC READINESS: CONDITIONAL",
		"// CORE LINK: SYNCHRONISED",
		"// COMMS: ENCRYPTED // BAND 7",
		"// SIGNAL INTERCEPT: PARTIAL",
		"// SYSTEM CLOCK: DRIFTING",
		"// DATA BUS: SATURATED",
		"// AIRSPACE: QUIET",
		"// SATLINK: DEGRADED (MINOR)",
		"// THERMAL: WITHIN TOLERANCE",
		"// SECTOR 4: NO CONTACT",
		"// FUEL CELLS: 87%",
		"// RADAR DOME: ROTATING",
		"// SIGNAL NOISE: LOW",
		"// CIVILIAN CORRIDOR: OPEN",
		"// CONDITION: AMBER",
		"// PERIMETER DRONES: IDLE",
		"// BUNKER SEAL: VERIFIED",
		"// LAST UPDATE: 0347 LOCAL",
		"// INPUT BUFFER: OVERFLOW",
		"// TIME TO DAWN: 04:12:33",
		"// COLD WAR // STILL ON",
		"// TARGET VECTOR: ERRATIC",
		"// ELECTRONIC WARFARE: DORMANT",
		"// LISTENING POST: ACTIVE",
		"// BACKGROUND SCAN: CLEAN",
		"// LINE TO COMMAND: OPEN",
		"// OVERRIDE LOCK: DISENGAGED",
		"// SYSTEM INTEGRITY: 98.4%",
		"// WATCHDOG TIMER: RUNNING",
		"// COOLANT LOOP: CYCLING",
		"// DISPLAY // IMAGE.JPEG",
		"// DEAD ZONE MAPPING: INCOMPLETE",
		"// NO-FLY BEACONS: ONLINE",
	};

	public static readonly string[] TreeEntries = new[]
	{
		"/SYS",
		"├── boot.cfg",
		"├── kernel.img",
		"├── watchdog.bin",
		"└── checksum.dat",
		"/SAM",
		"├── site_01.cfg",
		"├── site_02.cfg",
		"├── site_03.cfg",
		"├── site_04.cfg",
		"├── /AMMO",
		"│   ├── stockpile.dat",
		"│   ├── reload_queue.bin",
		"│   └── manifest.log",
		"├── /ARCS",
		"│   ├── arc_01.json",
		"│   ├── arc_02.json",
		"│   ├── arc_02 (Copy).json",
		"│   └── overlap.dat",
		"└── fire_control.cfg",
		"/IFF",
		"├── friendly.db",
		"├── transponder.bin",
		"├── decoder_mk2.cfg",
		"├── /SPOOF",
		"│   ├── filter.bin",
		"│   ├── known_signatures.dat",
		"│   └── confidence.tbl",
		"└── threshold.cfg",
		"/COMMS",
		"├── freq_table.dat",
		"├── encryption.key",
		"├── /LOG",
		"│   ├── 0347.log",
		"│   ├── 0312.log",
		"│   ├── 0246.log",
		"│   └── 0144.log",
		"└── /CHANNELS",
		"    ├── ch_01.cfg",
		"    ├── ch_04.cfg",
		"    └── ch_07.cfg",
		"/RDR",
		"├── sweep.cfg",
		"├── range.cfg",
		"├── /FILTERS",
		"│   ├── jamming.bin",
		"│   ├── ghost.dat",
		"│   └── decoy.dat",
		"└── dome_motor.cfg",
		"/AUTH",
		"├── operator_07.key",
		"├── operator_07_old.key",
		"├── command.cert",
		"├── /BIOMETRIC",
		"│   ├── retina.dat",
		"│   └── fingerprint.dat",
		"└── access.tbl",
		"/ENV",
		"├── thermal.cfg",
		"├── pressure.cfg",
		"├── humidity.cfg",
		"├── co2.cfg",
		"└── radiation.dat",
		"/PWR",
		"├── generator.cfg",
		"├── solar.cfg",
		"├── /BACKUP",
		"│   ├── battery_01.cfg",
		"│   ├── battery_02.cfg",
		"│   └── capacitor.cfg",
		"└── load_balance.cfg",
		"/SEC",
		"├── perimeter.cfg",
		"├── perimeter.cfg.old",
		"├── door_codes.dat",
		"├── /CAMERAS",
		"│   ├── feed_01.cfg",
		"│   ├── feed_02.cfg",
		"│   └── nightvision.cfg",
		"└── alert_protocol.cfg",
		"/CMD",
		"├── orders.queue",
		"├── /BRIEFINGS",
		"│   ├── 0420.txt",
		"│   ├── 0421_new.txt",
		"│   └── 0422.txt",
		"└── escalation.tbl",
		"/MAINT",
		"├── filter_log.dat",
		"├── lubrication.log",
		"├── flicker_report.txt",
		"└── /SPARE",
		"    ├── parts.inv",
		"    └── tools.inv",
		"/MISC",
		"├── kettle.log",
		"├── cat_sightings.txt",
		"├── crisp_inventory.dat",
		"├── humming_report.txt",
		"├── ventilation_anomaly.log",
		"└── DO_NOT_OPEN.dat",
	};

	public static readonly string[] LogMessages = new[]
	{
		"[ SYS ] connection established",
		"[ NET ] handshake ok",
		"[ SYS ] redundancy systems engaged",
		"[ AUTH ] operator_07 verified",
		"[ NET ] satellite link acquired",
		"[ RDR ] sweep cycle nominal",
		"[ IFF ] decoder online",
		"[ SAM ] all sites green",
		"[ SYS ] memory integrity: 99.98%",
		"[ SYS ] awaiting input",
		"[ ERR ] size 2 fish caught in ventilation",
		"[ NET ] uplink to command: stable",
		"[ PWR ] generator output: 92%",
		"[ SAM ] site 03 reload complete",
		"[ RDR ] dome rotation: 6 rpm",
		"[ COMMS ] freq scan complete",
		"[ SYS ] checksum mismatch detected",
		"[ ENV ] bunker temp: 14.2 c",
		"[ IFF ] friendly db updated: 142 entries",
		"[ AUTH ] biometric scan passed",
		"[ SAM ] site 01 ammo: 8/12",
		"[ DEBUG ] tickrate stable",
		"[ NET ] heartbeat received",
		"[ WARN ] coffee machine offline",
		"[ SYS ] auto-archive complete",
		"[ NET ] packet loss: negligible",
		"[ SEC ] camera feed 03 lost",
		"[ ERR ] sensor 04 drift detected",
		"[ PWR ] backup batteries: 100%",
		"[ NET ] routing table updated",
		"[ NET ] last sync: 0312",
		"[ CMD ] orders: hold position",
		"[ WARN ] anomalous echo detected at low altitude",
		"[ NET ] relay station heartbeat missing",
		"[ SYS ] fallback grid engaged",
		"[ ENV ] humidity: 38%",
		"[ MAINT ] filter change due in 14d",
		"[ SAM ] site 02 muzzle cover deployed",
		"[ SEC ] door 4 access: operator only",
		"[ WARN ] vending machine: out of crisps",
		"[ NET ] decryption key refreshed",
		"[ MISC ] cat reported in maintenance shaft",
		"[ RDR ] no contacts on scope",
		"[ SYS ] log buffer rotated",
		"[ COMMS ] silence on channel 4",
		"[ ERR ] clock drift: +0.03s",
		"[ SAM ] hydraulic pressure nominal",
		"[ ENV ] CO2 scrubber: running",
		"[ IFF ] spoofing filter: enabled",
		"[ PWR ] solar trickle: minimal",
		"[ MAINT ] light flicker logged sector 2",
		"[ MISC ] someone left the kettle on",
		"[ SYS ] boot sequence complete",
		"[ NET ] signal latency within tolerance",
		"[ RDR ] long-range sweep initiated",
		"[ IFF ] false positive rate nominal",
		"[ SAM ] site 04 standby mode enabled",
		"[ PWR ] capacitor banks charging",
		"[ COMMS ] signal clarity restored",
		"[ ENV ] radiation baseline stable",
		"[ SEC ] perimeter scan complete",
		"[ WARN ] morale index below optimal threshold",
		"[ ERR ] memory leak contained",
		"[ MAINT ] lubrication cycle complete",
		"[ CMD ] escalation protocol locked",
		"[ MISC ] unexplained humming in lower bunker",
		"[ SYS ] uptime: 147 days",
		"[ SYS ] watchdog timer reset",
	};
}
