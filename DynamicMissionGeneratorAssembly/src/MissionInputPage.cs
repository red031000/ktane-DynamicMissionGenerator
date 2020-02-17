using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class MissionInputPage : MonoBehaviour
	{
		public KMSelectable RunButtonSelectable;
		public InputField InputField;
		public KMGameCommands GameCommands;
		public ModuleListItem ModuleListItemPrefab;
		public RectTransform ModuleList;
		public Scrollbar Scrollbar;
		private readonly List<GameObject> listItems = new List<GameObject>();

		public KMAudio Audio;
		public KMGameInfo GameInfo;

		private static readonly List<ModuleData> moduleData = new List<ModuleData>();
		private static readonly Regex tokenRegex = new Regex(@"
			\G(?:^\s*|\s+)()(?:  # Group 1 marks the position after whitespace.
				(?:(?<Hr>\d{1,9}):)?(?<Min>\d{1,9}):(?<Sec>\d{1,9})|  # Bomb time
				(?<Strikes>\d{1,9})X\b|  # Strike limit
				(?<Setting>strikes|needyactivationtime|widgets|nopacing|frontonly)(?:\:(?<Value>\d{0,9}))?|  # Setting
				(?:(?<Count>\d{1,9})[;*])?  # Module pool count
				(?<ID>(?:[^\s""]|""[^""]*(?:""|$))+)  # Module IDs
			)?(?!\S)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

		private int tabListIndex = -1;
		private int tabCursorPosition = -1;
		private string tabStub;
		private bool tabProcessing;

		public void Start()
		{
			RunButtonSelectable.OnInteract += RunInteract;
			_elevatorRoomType = ReflectionHelper.FindType("ElevatorRoom");
			_gameplayStateType = ReflectionHelper.FindType("GameplayState");
			if (_gameplayStateType != null)
				_gameplayroomPrefabOverrideField = _gameplayStateType.GetField("GameplayRoomPrefabOverride", BindingFlags.Public | BindingFlags.Static);

			// KMModSettings is not used here because this isn't strictly a configuration option.
			string path = Path.Combine(Application.persistentDataPath, "LastDynamicMission.txt");
			if (File.Exists(path)) InputField.text = File.ReadAllText(path);
		}

		public void Update()
		{
			if (EventSystem.current.currentSelectedGameObject == InputField.gameObject)
			{
				if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
					RunInteract();
				if (Input.GetKeyDown(KeyCode.Tab) && listItems.Count > 0)
				{
					tabProcessing = true;
					if (tabListIndex >= 0 && tabListIndex < listItems.Count)
						SetNormalColour(listItems[tabListIndex].GetComponent<Button>(), tabListIndex % 2 == 0 ? Color.white : new Color(0.875f, 0.875f, 0.875f));
					if (listItems.Count == 0 || string.IsNullOrEmpty(listItems[0].GetComponent<ModuleListItem>().ID))
					{
						tabProcessing = false;
						return;
					}
					if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
					{
						if (tabListIndex < 0) tabListIndex = listItems.Count;
						--tabListIndex;
					} else
					{
						++tabListIndex;
						if (tabListIndex >= listItems.Count) tabListIndex = -1;
					}
					if (tabListIndex < 0)
					{
						tabCursorPosition = ReplaceToken(tabStub, false);
						Scrollbar.value = 1;
					}
					else
					{
						SetNormalColour(listItems[tabListIndex].GetComponent<Button>(), new Color(1, 0.75f, 1));
						string id = listItems[tabListIndex].GetComponent<ModuleListItem>().ID;
						tabCursorPosition = ReplaceToken(id, false);
						float offset = (-((RectTransform) ModuleList.parent).rect.height + ((RectTransform) ModuleListItemPrefab.transform).sizeDelta.y * (tabListIndex * 2 + 1)) / 2;
						float limit = ModuleList.rect.height - ((RectTransform) ModuleList.parent).rect.height;
						Scrollbar.value = Math.Min(1, 1 - offset / limit);
					}
				}
			}
		}

		private static void SetNormalColour(Selectable selectable, Color color)
		{
			var colours = selectable.colors;
			colours.normalColor = color;
			selectable.colors = colours;
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

			bool success = ParseTextToMission(InputField.text, out KMMission mission, out var messages);
			if (!success)
			{
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				foreach (var item in listItems) Destroy(item);
				listItems.Clear();
				foreach (string m in messages)
				{
					var item = Instantiate(ModuleListItemPrefab, ModuleList);
					item.Name = m;
					item.ID = "";
					listItems.Add(item.gameObject);
				}
				return false;
			}

			try
			{
				File.WriteAllText(Path.Combine(Application.persistentDataPath, "LastDynamicMission.txt"), InputField.text);
			}
			catch (Exception ex)
			{
				Debug.LogError("[Dynamic Mission Generator] Could not write LastDynamicMission.txt");
				Debug.LogException(ex, this);
			}
			GameCommands.StartMission(mission, "-1");

			return false;
		}

		public void TextChanged(string newText)
		{
			if (tabProcessing) return;
			tabListIndex = -1;
			tabCursorPosition = InputField.caretPosition;

			if (InputField.caretPosition >= 2 && newText[InputField.caretPosition - 1] == ',' && newText[InputField.caretPosition - 2] == ' ')
			{
				// If a comma was typed immediately after an auto-inserted space, remove the space.
				StartCoroutine(SetSelectionCoroutine(InputField.caretPosition - 1));
				InputField.text = newText.Remove(InputField.caretPosition - 2, 1);
				return;
			}

			foreach (var item in listItems) Destroy(item);
			listItems.Clear();
			if (moduleData.Count == 0) InitModules();

			var matches = tokenRegex.Matches(newText.Substring(0, InputField.caretPosition));
			if (matches.Count > 0)
			{
				var lastMatch = matches[matches.Count - 1];
				if (lastMatch.Groups["Min"].Success)
				{
					string text = $"Time: {(lastMatch.Groups["Hr"].Success ? int.Parse(lastMatch.Groups["Hr"].Value) + "h " : "")}{int.Parse(lastMatch.Groups["Min"].Value)}m {int.Parse(lastMatch.Groups["Sec"].Value)}s";
					var item = AddListItem("", text, false);
					item.HighlightName(6, text.Length - 6);
				}
				else if (lastMatch.Groups["Strikes"].Success)
				{
					var item = AddListItem("", "Strike limit: " + lastMatch.Value.TrimStart(), false);
					item.HighlightName(14, item.Name.Length - 14);
				}
				else if (lastMatch.Groups["Setting"].Success)
				{
					var item = AddListItem("", lastMatch.Groups["Setting"].Value + ": " + lastMatch.Groups["Value"].Value, false);
					item.HighlightName(lastMatch.Groups["Setting"].Value.Length + 2, item.Name.Length - (lastMatch.Groups["Setting"].Value.Length + 2));
				}
				else if (lastMatch.Groups["ID"].Success)
				{
					string s = GetLastModuleID(lastMatch.Groups["ID"].Value).Replace("\"", "");
					tabStub = s;
					if (!lastMatch.Groups["Count"].Success && !string.IsNullOrEmpty(lastMatch.Groups["ID"].Value) && lastMatch.Groups["ID"].Value.All(char.IsDigit))
					{
						var item = AddListItem($"{s}:00", "[Set time]", true);
						item.HighlightID(0, s.Length);
						item = AddListItem($"{s}X", "[Set strike limit]", true);
						item.HighlightID(0, s.Length);
						item = AddListItem($"{s}*", "[Set module pool count]", true);
						item.HighlightID(0, s.Length);
					}
					foreach (var m in moduleData)
					{
						bool id = m.ModuleType.StartsWith(s, StringComparison.InvariantCultureIgnoreCase);
						bool name = !id && m.DisplayName.StartsWith(s, StringComparison.InvariantCultureIgnoreCase);
						if (id || name)
						{
							var item = AddListItem(m.ModuleType, m.DisplayName, true);
							if (id) item.HighlightID(0, s.Length);
							else if (name) item.HighlightName(0, s.Length);
						}
					}
				}
			}
		}

		private static string GetLastModuleID(string list) => list.Substring(GetLastModuleIDPos(list));
		private static int GetLastModuleIDPos(string list) => list.LastIndexOfAny(new[] { ',', '+' }) + 1;

		private ModuleListItem AddListItem(string id, string text, bool addClickEvent)
		{
			var item = Instantiate(ModuleListItemPrefab, ModuleList);
			if (listItems.Count % 2 != 0) SetNormalColour(item.GetComponent<Button>(), new Color(0.875f, 0.875f, 0.875f));
			if (addClickEvent) item.Click += ModuleListItem_Click;
			item.Name = text;
			item.ID = id;
			listItems.Add(item.gameObject);
			return item;
		}

		private void ModuleListItem_Click(object sender, EventArgs e)
		{
			string id = ((ModuleListItem) sender).ID;
			tabProcessing = true;
			ReplaceToken(id, !id.EndsWith("*"));
			tabProcessing = false;
		}

		private int ReplaceToken(string id, bool space)
		{
			var match = tokenRegex.Matches(InputField.text.Substring(0, tabCursorPosition)).Cast<Match>().Last();

			int startIndex;
			if (match.Groups["ID"].Success)
			{
				startIndex = match.Groups["ID"].Index + GetLastModuleIDPos(match.Groups["ID"].Value);
				if (id.Contains(' ') && match.Groups["ID"].Value.Take(startIndex).Count(c => c == '"') % 2 == 0)
					id = "\"" + id + "\"";
				if (space) id += " ";
			}
			else startIndex = match.Groups[1].Index;
			InputField.text = InputField.text.Remove(startIndex, tabCursorPosition - startIndex).Insert(startIndex, id);
			InputField.Select();
			StartCoroutine(SetSelectionCoroutine(startIndex + id.Length));
			return startIndex + id.Length;
		}

		private IEnumerator SetSelectionCoroutine(int pos)
		{
			yield return null;
			InputField.caretPosition = pos;
			InputField.ForceLabelUpdate();
			if (tabProcessing) tabProcessing = false;
			else TextChanged(InputField.text);
		}

		private static void InitModules()
		{
			moduleData.Add(new ModuleData("ALL_SOLVABLE", "[All solvable modules]"));
			moduleData.Add(new ModuleData("ALL_NEEDY", "[All needy modules]"));
			moduleData.Add(new ModuleData("ALL_MODS", "[All mod modules]"));
			moduleData.Add(new ModuleData("frontonly", "[Front face only]"));
			moduleData.Add(new ModuleData("nopacing", "[Disable pacing events]"));
			moduleData.Add(new ModuleData("widgets:", "[Set widget count]"));
			moduleData.Add(new ModuleData("needyactivationtime:", "[Set needy activation time in seconds]"));
			moduleData.Add(new ModuleData("Wires", "Wires"));
			moduleData.Add(new ModuleData("Keypad", "Keypad"));
			moduleData.Add(new ModuleData("Memory", "Memory"));
			moduleData.Add(new ModuleData("Maze", "Maze"));
			moduleData.Add(new ModuleData("Password", "Password"));
			moduleData.Add(new ModuleData("BigButton", "The Button"));
			moduleData.Add(new ModuleData("Simon", "Simon Says"));
			moduleData.Add(new ModuleData("WhosOnFirst", "Who's On First"));
			moduleData.Add(new ModuleData("Morse", "Morse Code"));
			moduleData.Add(new ModuleData("Venn", "Complicated Wires"));
			moduleData.Add(new ModuleData("WireSequence", "Wire Sequence"));
			moduleData.Add(new ModuleData("NeedyVentGas", "Venting Gas"));
			moduleData.Add(new ModuleData("NeedyCapacitor", "Capacitor Discharge"));
			moduleData.Add(new ModuleData("NeedyKnob", "Knob"));

			if (Application.isEditor)
			{
				moduleData.Add(new ModuleData($"Space Test", $"Space Test"));
				for (int i = 0; i < 30; ++i)
				{
					moduleData.Add(new ModuleData($"ScrollTest{i:00}", $"Scroll Test {i}"));
				}
			}

			if (DynamicMissionGenerator.ModSelectorApi != null)
			{
				var assembly = DynamicMissionGenerator.ModSelectorApi.GetType().Assembly;
				var serviceType = assembly.GetType("ModSelectorService");
				object service = serviceType.GetProperty("Instance").GetValue(null, null);
				var allSolvableModules = (IDictionary) serviceType.GetField("_allSolvableModules", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(service);
				var allNeedyModules = (IDictionary) serviceType.GetField("_allNeedyModules", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(service);

				foreach (object entry in allSolvableModules.Cast<object>().Concat(allNeedyModules.Cast<object>()))
				{
					string id = (string) entry.GetType().GetProperty("Key").GetValue(entry, null);
					object value = entry.GetType().GetProperty("Value").GetValue(entry, null);
					string name = (string) value.GetType().GetProperty("ModuleName").GetValue(value, null);
					moduleData.Add(new ModuleData(id, name));
				}
			}

			moduleData.Sort((a, b) => a.ModuleType.CompareTo(b.ModuleType));
		}

		private bool ParseTextToMission(string text, out KMMission mission, out List<string> messages)
		{
			messages = new List<string>();

			var matches = tokenRegex.Matches(text);
			if (matches.Count == 0 || (matches.Count == 1 && !matches[0].Value.Any(c => !char.IsWhiteSpace(c))) || matches[matches.Count - 1].Index + matches[matches.Count - 1].Length < text.Length)
			{
				messages.Add("Syntax error");
				mission = null;
				return false;
			}

			bool timeSpecified = false, strikesSpecified = false, anySolvableModules = false;
			mission = ScriptableObject.CreateInstance<KMMission>();
			mission.PacingEventsEnabled = true;
			mission.GeneratorSetting = new KMGeneratorSetting();
			List<KMComponentPool> pools = new List<KMComponentPool>();
			foreach (Match match in matches)
			{
				if (match.Groups["Min"].Success)
				{
					if (timeSpecified)
						messages.Add("Time specified multiple times");
					else
					{
						timeSpecified = true;
						mission.GeneratorSetting.TimeLimit = (match.Groups["Hr"].Success ? int.Parse(match.Groups["Hr"].Value) * 3600 : 0) +
							int.Parse(match.Groups["Min"].Value) * 60 + int.Parse(match.Groups["Sec"].Value);
						if (mission.GeneratorSetting.TimeLimit <= 0) messages.Add("Invalid time limit");
					}
				}
				else if (match.Groups["Strikes"].Success)
				{
					if (strikesSpecified) messages.Add("Strike limit specified multiple times");
					else
					{
						strikesSpecified = true;
						mission.GeneratorSetting.NumStrikes = int.Parse(match.Groups["Strikes"].Value);
						if (mission.GeneratorSetting.NumStrikes <= 0) messages.Add("Invalid strike limit");
					}
				}
				else if (match.Groups["Setting"].Success)
				{
					switch (match.Groups["Setting"].Value.ToLowerInvariant())
					{
						case "strikes":
							if (match.Groups["Value"].Success) {
								if (strikesSpecified) messages.Add("Strike limit specified multiple times");
								else
								{
									strikesSpecified = true;
									mission.GeneratorSetting.NumStrikes = int.Parse(match.Groups["Value"].Value);
									if (mission.GeneratorSetting.NumStrikes <= 0) messages.Add("Invalid strike limit");
								}
							}
							break;
						case "needyactivationtime":
							if (match.Groups["Value"].Success) mission.GeneratorSetting.TimeBeforeNeedyActivation = int.Parse(match.Groups["Value"].Value);
							break;
						case "widgets":
							if (match.Groups["Value"].Success) mission.GeneratorSetting.OptionalWidgetCount = int.Parse(match.Groups["Value"].Value);
							break;
						case "nopacing": mission.PacingEventsEnabled = false; break;
						case "frontonly": mission.GeneratorSetting.FrontFaceOnly = true; break;
					}
				}
				else if (match.Groups["ID"].Success)
				{
					KMComponentPool pool = new KMComponentPool
					{
						Count = match.Groups["Count"].Success ? int.Parse(match.Groups["Count"].Value) : 1,
						ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
						ModTypes = new List<string>()
					};
					if (pool.Count <= 0) messages.Add("Invalid module pool count");

					bool allSolvable = true;
					string list = match.Groups["ID"].Value.Replace("\"", "").Trim();
					switch (list)
					{
						case "ALL_SOLVABLE":
							anySolvableModules = true;
							pool.AllowedSources = KMComponentPool.ComponentSource.Base | KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_SOLVABLE;
							break;
						case "ALL_NEEDY":
							pool.AllowedSources = KMComponentPool.ComponentSource.Base | KMComponentPool.ComponentSource.Mods;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_NEEDY;
							break;
						case "ALL_VANILLA":
							anySolvableModules = true;
							pool.AllowedSources = KMComponentPool.ComponentSource.Base;
							pool.SpecialComponentType = KMComponentPool.SpecialComponentTypeEnum.ALL_SOLVABLE;
							break;
						case "ALL_MODS":
							anySolvableModules = true;
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
							foreach (string id in list.Split(',', '+').Select(s => s.Trim()))
							{
									switch (id)
									{
										case "WireSequence": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.WireSequence); break;
										case "Wires": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Wires); break;
										case "WhosOnFirst": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.WhosOnFirst); break;
										case "Simon": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Simon); break;
										case "Password": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Password); break;
										case "Morse": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Morse); break;
										case "Memory": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Memory); break;
										case "Maze": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Maze); break;
										case "Keypad": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Keypad); break;
										case "Venn": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.Venn); break;
										case "BigButton": pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.BigButton); break;
										case "NeedyCapacitor":
											allSolvable = false;
											pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.NeedyCapacitor);
											break;
										case "NeedyVentGas":
											allSolvable = false;
											pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.NeedyVentGas);
											break;
										case "NeedyKnob":
											allSolvable = false;
											pool.ComponentTypes.Add(KMComponentPool.ComponentTypeEnum.NeedyKnob);
											break;
										default:
											if (!((IEnumerable<string>) DynamicMissionGenerator.ModSelectorApi["AllSolvableModules"]).Contains(id) &&
												!((IEnumerable<string>) DynamicMissionGenerator.ModSelectorApi["AllNeedyModules"]).Contains(id))
												messages.Add($"'{id}' is an unknown module ID.");
											else if (((IEnumerable<string>) DynamicMissionGenerator.ModSelectorApi["DisabledSolvableModules"]).Contains(id) ||
												((IEnumerable<string>) DynamicMissionGenerator.ModSelectorApi["DisabledNeedyModules"]).Contains(id))
												messages.Add($"'{id}' is disabled.");
											else
											{
												if (((IEnumerable<string>) DynamicMissionGenerator.ModSelectorApi["AllNeedyModules"]).Contains(id))
													allSolvable = false;
												pool.ModTypes.Add(id);
											}
											break;
									}
							}
							break;
					}
					if (allSolvable) anySolvableModules = true;
					if (pool.ModTypes.Count == 0)
						pool.ModTypes = null;
					if (pool.ComponentTypes.Count == 0)
						pool.ComponentTypes = null;
					pools.Add(pool);
				}
			}

			if (!anySolvableModules) messages.Add("No regular modules");
			mission.GeneratorSetting.ComponentPools = pools;
			if (mission.GeneratorSetting.GetComponentCount() > GetMaxModules())
				messages.Add($"Too many modules for any available bomb casing ({mission.GeneratorSetting.GetComponentCount()} specified; {GetMaxModules()} possible).");

			if (messages.Count > 0)
			{
				Destroy(mission);
				mission = null;
				return false;
			}
			messages = null;
			mission.DisplayName = "Custom Freeplay";
			if (!timeSpecified) mission.GeneratorSetting.TimeLimit = mission.GeneratorSetting.GetComponentCount() * 120;
			if (!strikesSpecified) mission.GeneratorSetting.NumStrikes = Math.Max(3, mission.GeneratorSetting.GetComponentCount() / 12);
			return true;
		}

		private int GetMaxModules()
		{
			GameObject roomPrefab = (GameObject) _gameplayroomPrefabOverrideField.GetValue(null);
			if (roomPrefab == null) return GameInfo.GetMaximumBombModules();
			return roomPrefab.GetComponentInChildren(_elevatorRoomType, true) != null ? 54 : GameInfo.GetMaximumBombModules();
		}

		private static Type _gameplayStateType;
		private static FieldInfo _gameplayroomPrefabOverrideField;

		private static Type _elevatorRoomType;
	}
}
