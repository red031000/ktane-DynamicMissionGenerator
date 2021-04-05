using System.Collections.Generic;

class DMGMission
{
	public KMMission KMMission;
	public int? RuleSeed;
	public Mode Mode;
	public string GameplayRoom;
	public List<string> Messages;
}

public enum Mode
{
	Normal,
	Time,
	Zen,
	Steady
}