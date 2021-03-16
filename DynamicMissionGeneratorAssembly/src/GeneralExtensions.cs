using System.Collections.Generic;
using System.Linq;

namespace DynamicMissionGeneratorAssembly
{
	internal static class GeneralExtensions
	{
		public static bool EqualsAny(this string tester, params string[] strings)
		{
			return strings.Contains(tester);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(_ => UnityEngine.Random.value);
		}
	}
}
