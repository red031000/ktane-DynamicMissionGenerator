using System;

namespace DynamicMissionGeneratorAssembly
{
	public class ModuleData
	{
		public string ModuleType { get; }
		public string DisplayName { get; }

		public ModuleData(string moduleType, string displayName)
		{
			ModuleType = moduleType ?? throw new ArgumentNullException(nameof(moduleType));
			DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
		}
	}
}
