using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class RunButton : MonoBehaviour
	{
		public KMSelectable RunButtonSelectable;
		public InputField InputField;
		public KMGameCommands GameCommands;

		public KMAudio Audio;
		public KMGameInfo GameInfo;

		private void Awake()
		{
			RunButtonSelectable.OnInteract += RunInteract;
		}

		private void Start()
		{
			_elevatorRoomType = ReflectionHelper.FindType("ElevatorRoom");
			_gameplayStateType = ReflectionHelper.FindType("GameplayState");
			if (_gameplayStateType != null)
				_gameplayroomPrefabOverrideField = _gameplayStateType.GetField("GameplayRoomPrefabOverride", BindingFlags.Public | BindingFlags.Static);
		}

		private bool RunInteract()
		{
			if (InputField == null)
				return false;
			if (string.IsNullOrEmpty(InputField.text))
			{
				Debug.Log("Empty");
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				return false;
			}

			bool success = ParseTextToMission(InputField.text, out KMMission mission);
			if (!success)
			{
				Debug.Log("Returned no success");
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				return false;
			}

			GameCommands.StartMission(mission, "-1");

			return false;
		}

		private bool ParseTextToMission(string text, out KMMission mission)
		{
			mission = null;
			List<Tuple<int, string[]>> tuples = new List<Tuple<int, string[]>>();
			try
			{
				Match match = Regex.Match(text, @"(?:(\d+);""(\S+)"" ?)+");
				if (match.Success)
				{
					Debug.Log("SUCCESS!");
					Debug.Log("Group count: " + match.Groups.Count);
					Debug.Log("Capture count: " + match.Groups[1].Captures.Count);
					for (int i = 0; i < match.Groups[1].Captures.Count; i++)
					{
						Debug.Log("___MARKER3___");
						Debug.Log(match.Groups[1].Captures[i].Value);
						Debug.Log(match.Groups[2].Captures[i].Value);
						if (!int.TryParse(match.Groups[1].Captures[i].Value, out int parsed))
						{
							Debug.Log("FAKE NEWS");
							Debug.Log(match.Groups[1].Captures[i].Value);
							return false;
						}

						string[] string2 = match.Groups[2].Captures[i].Value.Split('+');
						tuples.Add(new Tuple<int, string[]>(parsed, string2));
					}
				}
				else
				{
					Debug.Log("NO SUCCESS");
					return false;
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message + Environment.NewLine + e.StackTrace);
				return false;
			}

			bool marker = false;
			foreach (Tuple<int, string[]> tuple in tuples)
			{
				foreach (string item in tuple.Second)
				{
					Debug.Log("___MARKER2___");
					Debug.Log(item);
					bool marker2 = false;
					bool marker3 = false;
					if (((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["AllSolvableModules"]).Contains(item) &&
					    !((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["DisabledSolvableModules"]).Contains(item))
						marker = true;
					if (item.EqualsAny("BigButton", "Venn", "Keypad", "Maze", "Memory", "Morse", "Password",
						"Simon", "WhosOnFirst", "Wires", "WireSequence", "ALL_SOLVABLE", "ALL_VANILLA", "ALL_MODS"))
					{
						marker = true;
						marker3 = true;
					}

					if (item.EqualsAny("NeedyVentGas", "NeedyKnob", "NeedyCapacitor", "ALL_NEEDY", "ALL_VANILLA_NEEDY", "ALL_MODS_NEEDY"))
						marker2 = true;
					if ((!((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["AllSolvableModules"]).Contains(item) && //mod not loaded as solvable
					    !((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["AllNeedyModules"]).Contains(item) || //and mod not loaded as needy
						((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["DisabledSolvableModules"]).Contains(item) || //or mod is a disabled solvable
						((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["DisabledNeedyModules"]).Contains(item)) //or mod is a disabled needy
						&& !marker3 && !marker2) //and it's not in the exclusion lists above
						return false;
				}

				if ((tuple.Second.Contains("ALL_SOLVABLE") || tuple.Second.Contains("ALL_MODS") || tuple.Second.Contains("ALL_VANILLA")) && tuple.Second.Length != 1)
					return false;
			}

			if (!marker)
				return false;
			
			if (!tuples.Any(x => x.Second.Any(x2 => !x2.EqualsAny("NeedyVentGas", "NeedyKnob", "NeedyCapacitor", "ALL_NEEDY", "ALL_VANILLA_NEEDY", "ALL_MODS_NEEDY"))))
				return false;
				
			mission = GenerateMission(tuples);

			return mission != null;
		}

		private KMMission GenerateMission(IEnumerable<Tuple<int, string[]>> tuples)
		{
			int modules = 0;
			List<KMComponentPool> pools = new List<KMComponentPool>();
			foreach (Tuple<int, string[]> tuple in tuples)
			{
				modules += tuple.First;
				KMComponentPool pool = new KMComponentPool
				{
					Count = tuple.First,
					ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
					ModTypes = new List<string>()
				};
				foreach (string item in tuple.Second)
				{
					Debug.Log("___MARKER___");
					Debug.Log(item);
					switch (item)
					{
						case "WireSequence":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.WireSequence);
							break;
						case "Wires":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Wires);
							break;
						case "WhosOnFirst":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.WhosOnFirst);
							break;
						case "Simon":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Simon);
							break;
						case "NeedyVentGas":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.NeedyVentGas);
							break;
						case "Password":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Password);
							break;
						case "Morse":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Morse);
							break;
						case "Memory":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Memory);
							break;
						case "Maze":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Maze);
							break;
						case "NeedyKnob":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.NeedyKnob);
							break;
						case "Keypad":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Keypad);
							break;
						case "Venn":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Venn);
							break;
						case "NeedyCapacitor":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.NeedyCapacitor);
							break;
						case "BigButton":
							pool.AllowedSources |= KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.BigButton);
							break;
						case "ALL_SOLVABLE":
							pool.AllowedSources = KMComponentPool.ComponentSource.Base | KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_SOLVABLE;
							break;
						case "ALL_NEEDY":
							pool.AllowedSources = KMComponentPool.ComponentSource.Base | KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_NEEDY;
							break;
						case "ALL_VANILLA":
							pool.AllowedSources = KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_SOLVABLE;
							break;
						case "ALL_MODS":
							pool.AllowedSources = KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_SOLVABLE;
							break;
						case "ALL_VANILLA_NEEDY":
							pool.AllowedSources = KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_NEEDY;
							break;
						case "ALL_MODS_NEEDY":
							pool.AllowedSources = KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_NEEDY;
							break;
						default:
							pool.AllowedSources |= KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None;
							pool.ModTypes.Add(item);
							break;
					}
				}

				if (pool.ModTypes.Count == 0)
				{
					pool.ModTypes = null;
				}

				if (pool.ComponentTypes.Count == 0)
				{
					pool.ComponentTypes = null;
				}

				pools.Add(pool);
			}

			KMMission mission = ScriptableObject.CreateInstance<KMMission>();
			mission.DisplayName = "Custom Freeplay";
			mission.PacingEventsEnabled = true;
			mission.GeneratorSetting = new KMGeneratorSetting
			{
				ComponentPools = pools,
				TimeLimit = modules * 120,
				NumStrikes = Math.Max(3, modules / 12)
			};
			return modules > GetMaxModules() ? null : mission;
		}

		private int GetMaxModules()
		{
			GameObject roomPrefab = (GameObject)_gameplayroomPrefabOverrideField.GetValue(null);
			if (roomPrefab == null) return GameInfo.GetMaximumBombModules();
			return roomPrefab.GetComponentInChildren(_elevatorRoomType, true) != null ? 54 : GameInfo.GetMaximumBombModules();
		}

		private static Type _gameplayStateType;
		private static FieldInfo _gameplayroomPrefabOverrideField;

		private static Type _elevatorRoomType;
	}
}
