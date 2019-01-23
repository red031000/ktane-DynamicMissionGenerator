using System.Linq;

namespace DynamicMissionGeneratorAssembly
{
	internal static class GeneralExtensions
	{
		public static bool EqualsAny(this string tester, params string[] strings)
		{
			return strings.Contains(tester);
		}
	}
}
