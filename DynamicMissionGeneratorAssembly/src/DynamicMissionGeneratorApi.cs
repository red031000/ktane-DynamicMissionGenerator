using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

namespace DynamicMissionGeneratorAssembly
{
	public class DynamicMissionGeneratorApi : MonoBehaviour, IDictionary<string, object>
	{
		public static DynamicMissionGeneratorApi Instance { get; private set; }

		public void Awake() => Instance = this;

		public ReadOnlyCollection<ReadOnlyCollection<string>> ModuleProfiles;

		public object this[string key]
		{
			get { if (key == nameof(ModuleProfiles)) return ModuleProfiles; throw new KeyNotFoundException(); }
			set { throw new NotSupportedException(); }
		}

		public ICollection<string> Keys { get; } = new ReadOnlyCollection<string>(new[] { nameof(ModuleProfiles) });
		public ICollection<object> Values => new ReadOnlyCollection<object>(new[] { ModuleProfiles });
		public int Count => 1;
		public bool IsReadOnly => true;

		public void Add(string key, object value) => throw new NotSupportedException();
		public void Add(KeyValuePair<string, object> item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Contains(KeyValuePair<string, object> item) => throw new NotImplementedException();
		public bool ContainsKey(string key) => throw new NotImplementedException();
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException();
		public bool Remove(string key) => throw new NotSupportedException();
		public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();
		public bool TryGetValue(string key, out object value) => throw new NotImplementedException();
		IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
	}
}
