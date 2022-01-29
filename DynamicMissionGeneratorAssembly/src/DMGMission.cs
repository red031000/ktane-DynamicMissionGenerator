using System.Collections.Generic;

public class DMGMission
{
	public KMMission KMMission;
	public int MissionSeed;
	public int? RuleSeed;
	public Mode Mode;
	public HUDMode BombHUD;
	public Dictionary<string, bool> TweakSettings;
	public Dictionary<string, float> ModeSettings;
	public string GameplayRoom;
	public List<string> Messages;
}

public enum HUDMode
{
	Off,
	Partial,
	On,
	None
}

public enum AdvantageousMode
{
	Off,
	Missions,
	On,
}

public enum Mode
{
	Normal,
	Time,
	Zen,
	Steady,
	None
}
