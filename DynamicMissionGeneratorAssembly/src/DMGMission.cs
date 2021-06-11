using System.Collections.Generic;

public class DMGMission
{
	public KMMission KMMission;
	public int MissionSeed;
	public int? RuleSeed;
	public Mode Mode;
	public Dictionary<string, bool> TweakSettings;
	public Dictionary<string, float> ModeSettings;
	public string GameplayRoom;
	public List<string> Messages;
}

public enum Mode
{
	Normal,
	Time,
	Zen,
	Steady,
	None
}
