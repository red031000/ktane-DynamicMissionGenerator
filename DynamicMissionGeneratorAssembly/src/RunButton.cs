using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				return false;
			}

			bool success = ParseTextToMission(InputField.text, out KMMission mission);
			if (!success)
			{
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				return false;
			}

			GameCommands.StartMission(mission, "-1");

			return false;
		}

		private bool ParseTextToMission(string text, out KMMission mission)
		{
			mission = null;
			List<Tuple<int, string>> tuples = new List<Tuple<int, string>>();
			try
			{
				string[] split1 = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string item in split1)
				{
					string[] split2 = item.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					bool result = int.TryParse(split2[0], out int parsed);
					if (!result)
						return false;

					tuples.Add(new Tuple<int, string>(parsed, split2[1]));
				}
			}
			catch
			{
				return false;
			}

			bool marker = false;
			foreach (Tuple<int, string> tuple in tuples)
			{
				bool marker2 = false;
				if (((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["AllSolvableModules"]).Contains(tuple.Second) &&
				    !((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["DisabledSolvableModules"]).Contains(tuple.Second))
					marker = true;
				if (tuple.Second.EqualsAny("BigButton", "Venn", "Keypad", "Maze", "Memory", "Morse", "Password",
					"Simon", "WhosOnFirst", "Wires", "WireSequence", "ALL_SOLVABLE"))
					marker = true;
				if (tuple.Second.EqualsAny("NeedyVentGas", "NeedyKnob", "NeedyCapacitor", "ALL_NEEDY"))
					marker2 = true;
				if (!(((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["AllSolvableModules"]).Contains(tuple.Second) && //mod not loaded as solvable
				    !((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["AllNeedyModules"]).Contains(tuple.Second) || //and mod not loaded as needy
					((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["DisabledSolvableModules"]).Contains(tuple.Second) || //or mod is a disabled solvable
					((IEnumerable<string>)DynamicMissionGenerator.ModSelectorApi["DisabledNeedyModules"]).Contains(tuple.Second)) //or mod is a disabled needy
					&& !marker && !marker2) //and it's not in the exclusion lists above
					return false;
			}

			if (!marker)
			{
				return false;
			}

			mission = GenerateMission(tuples);

			return mission != null;
		}

		private KMMission GenerateMission(IEnumerable<Tuple<int, string>> tuples)
		{
			int modules = 0;
			int van = 0;
			List<KMComponentPool> pools = new List<KMComponentPool>();
			foreach (Tuple<int, string> tuple in tuples)
			{
				modules += tuple.First;
				switch (tuple.Second)
				{
					case "WireSequence":
						KMComponentPool wsPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.WireSequence }
						};
						pools.Add(wsPool);
						van += tuple.First;
						break;
					case "Wires":
						KMComponentPool wPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Wires }
						};
						pools.Add(wPool);
						van += tuple.First;
						break;
					case "WhosOnFirst":
						KMComponentPool wofPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.WhosOnFirst }
						};
						pools.Add(wofPool);
						van += tuple.First;
						break;
					case "Simon":
						KMComponentPool sPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Simon }
						};
						pools.Add(sPool);
						van += tuple.First;
						break;
					case "NeedyVentGas":
						KMComponentPool vgPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.NeedyVentGas }
						};
						pools.Add(vgPool);
						van += tuple.First;
						break;
					case "Password":
						KMComponentPool pwPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Password }
						};
						pools.Add(pwPool);
						van += tuple.First;
						break;
					case "Morse":
						KMComponentPool mcPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Morse }
						};
						pools.Add(mcPool);
						van += tuple.First;
						break;
					case "Memory":
						KMComponentPool mPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Memory }
						};
						pools.Add(mPool);
						van += tuple.First;
						break;
					case "Maze":
						KMComponentPool maPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Maze }
						};
						pools.Add(maPool);
						van += tuple.First;
						break;
					case "NeedyKnob":
						KMComponentPool kPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.NeedyKnob }
						};
						pools.Add(kPool);
						van += tuple.First;
						break;
					case "Keypad":
						KMComponentPool kpPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Keypad }
						};
						pools.Add(kpPool);
						van += tuple.First;
						break;
					case "Venn":
						KMComponentPool vPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.Venn }
						};
						pools.Add(vPool);
						van += tuple.First;
						break;
					case "NeedyCapacitor":
						KMComponentPool cPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.NeedyCapacitor }
						};
						pools.Add(cPool);
						van += tuple.First;
						break;
					case "BigButton":
						KMComponentPool bbPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ComponentTypes = new List<KMComponentPool.ComponentTypeEnum> { KMComponentPool.ComponentTypeEnum.BigButton }
						};
						pools.Add(bbPool);
						van += tuple.First;
						break;
					case "ALL_SOLVABLE":
						KMComponentPool asPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base | KMComponentPool.ComponentSource.Mods,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_SOLVABLE,
						};
						pools.Add(asPool);
						break;
					case "ALL_NEEDY":
						KMComponentPool anPool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Base | KMComponentPool.ComponentSource.Mods,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_NEEDY,
						};
						pools.Add(anPool);
						break;
					default:
						KMComponentPool pool = new KMComponentPool
						{
							AllowedSources = KMComponentPool.ComponentSource.Mods,
							Count = tuple.First,
							SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.None,
							ModTypes = new List<string> {tuple.Second}
						};
						pools.Add(pool);
						break;
				}
			}

			KMMission mission = ScriptableObject.CreateInstance<KMMission>();
			mission.DisplayName = "Custom Freeplay";
			mission.PacingEventsEnabled = true;
			mission.GeneratorSetting = new KMGeneratorSetting
			{
				ComponentPools = pools,
				TimeLimit = modules * 120 - van * 60,
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
