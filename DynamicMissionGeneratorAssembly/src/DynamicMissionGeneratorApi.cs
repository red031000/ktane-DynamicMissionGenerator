#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using UnityEngine;

namespace DynamicMissionGeneratorAssembly
{
	public class DynamicMissionGeneratorApi : MonoBehaviour, IDictionary<string, IDictionary<string, IList<string?>>?>
	{
		public static DynamicMissionGeneratorApi Instance { get; private set; }

		public void Awake() => Instance = this;

		/// <summary>
		/// Returns a read-only dictionary mapping module IDs to a list specifying the associated profile for each instance of that module,
		/// or null if a Dynamic Mission Generator mission is not in progresss.
		/// </summary>
		/// <remarks>List items may be null, indicating the module instance was not chosen from a profile pool.</remarks>
		public ReadOnlyDictionary<string, IList<string?>>? ModuleProfiles { get; private set; }
		/// <summary>Stores the internal read-write lists for <see cref="ModuleProfiles"/>.</summary>
		private Dictionary<string, IList<string?>>? moduleProfiles;
		/// <summary>Stores the internal read-write lists for <see cref="ModuleProfiles"/>.</summary>
		private Dictionary<string, List<string?>>? privateModuleProfiles;
		internal List<string?>? firstBombProfileList;
		internal Queue<Queue<string?>>? upcomingBombProfileLists;
		internal Queue<string?>? currentBombProfileList;
		internal GameObject? currentBomb;

		internal void SetUpDynamicMissionGeneratorMission()
		{
			moduleProfiles = new();
			privateModuleProfiles = new();
			ModuleProfiles = new(moduleProfiles);
			firstBombProfileList = null;
			upcomingBombProfileLists = new();
			currentBombProfileList = null;
			currentBomb = null;
		}

		internal void AddProfileList(List<string?> profileList, int repeatCount)
		{
			if (upcomingBombProfileLists == null) throw new InvalidOperationException(nameof(SetUpDynamicMissionGeneratorMission) + " has not been called.");
			firstBombProfileList ??= profileList;
			for (; repeatCount > 0; --repeatCount)
				upcomingBombProfileLists.Enqueue(new(profileList));
		}

		internal void AddModule(string moduleID, string profile)
		{
			if (privateModuleProfiles == null || moduleProfiles == null) throw new InvalidOperationException(nameof(SetUpDynamicMissionGeneratorMission) + " has not been called.");
			if (!privateModuleProfiles.TryGetValue(moduleID, out var list))
			{
				list = new();
				privateModuleProfiles[moduleID] = list;
				moduleProfiles[moduleID] = list.AsReadOnly();
			}
			list.Add(profile);
		}

		internal Queue<string?>? GetNextProfileList()
			=> upcomingBombProfileLists == null ? null : upcomingBombProfileLists.Count > 0 ? upcomingBombProfileLists.Dequeue() : new(firstBombProfileList!);

		internal void FinishDynamicMissionGeneratorMission()
		{
			moduleProfiles = null;
			privateModuleProfiles = null;
			ModuleProfiles = null;
			firstBombProfileList = null;
			upcomingBombProfileLists = null;
			currentBombProfileList = null;
			currentBomb = null;
		}

		public bool InDynamicMissionGeneratorMission => moduleProfiles != null;

		public IDictionary<string, IList<string?>>? this[string key]
		{
			get => key == nameof(ModuleProfiles) ? ModuleProfiles : throw new KeyNotFoundException();
			set => throw new NotSupportedException();
		}

		public ICollection<string> Keys { get; } = new ReadOnlyCollection<string>(new[] { nameof(ModuleProfiles) });
		public ICollection<IDictionary<string, IList<string?>>?> Values => Array.AsReadOnly(new IDictionary<string, IList<string?>>?[] { ModuleProfiles });
		public int Count => 1;
		public bool IsReadOnly => true;
		public void Add(string key, IDictionary<string, IList<string?>>? value) => throw new NotSupportedException();
		public void Add(KeyValuePair<string, IDictionary<string, IList<string?>>?> item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Contains(KeyValuePair<string, IDictionary<string, IList<string?>>?> item) => throw new NotImplementedException();
		public bool ContainsKey(string key) => throw new NotImplementedException();
		public void CopyTo(KeyValuePair<string, IDictionary<string, IList<string?>>?>[] array, int arrayIndex) => throw new NotImplementedException();
		public IEnumerator<KeyValuePair<string, IDictionary<string, IList<string?>>?>> GetEnumerator() => throw new NotImplementedException();
		public bool Remove(string key) => throw new NotSupportedException();
		public bool Remove(KeyValuePair<string, IDictionary<string, IList<string?>>?> item) => throw new NotSupportedException();
		public bool TryGetValue(string key, out IDictionary<string, IList<string?>>? value) => throw new NotImplementedException();
		IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

		internal class BombProfileLists
		{
			public List<string> profilesFrontFace = new();
			public List<string> profilesAnyFace = new();
		}
	}

	// .NET Framework 3.5 predates the framework ReadOnlyDictionary type.
	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> dictionary;

		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) => this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

		public TValue this[TKey key] { get => dictionary[key]; set => throw new NotSupportedException(); }
		public ICollection<TKey> Keys => dictionary.Keys;
		public ICollection<TValue> Values => dictionary.Values;
		public int Count => dictionary.Count;
		public bool IsReadOnly => true;
		public void Add(TKey key, TValue value) => throw new NotSupportedException();
		public void Add(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Contains(KeyValuePair<TKey, TValue> item) => dictionary.Contains(item);
		public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => dictionary.CopyTo(array, arrayIndex);
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();
		public bool Remove(TKey key) => throw new NotSupportedException();
		public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();
		public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
