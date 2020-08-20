namespace DynamicMissionGeneratorAssembly
{
	public class Tuple<T1, T2>
	{
		public T1 First { get; }
		public T2 Second { get; }
		internal Tuple(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
	}
}