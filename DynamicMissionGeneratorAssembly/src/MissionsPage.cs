using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicMissionGeneratorAssembly
{
	public class MissionsPage : MonoBehaviour
	{
		public KMSelectable SwitchSelectable;
		public KMSelectable FolderSelectable;
		public ModuleListItem MissionListItem;
		public RectTransform MissionList;
		public GameObject NoMissionsText;
		public RectTransform ContextMenu;
		public RectTransform CanvasTransform;
		public Prompt Prompt;
		public Alert Alert;

		private string missionsFolder => DynamicMissionGenerator.MissionsFolder;
		private Dictionary<string, Mission> missions = new Dictionary<string, Mission>();
		private Mission contextMenuMission;

		public void Start()
		{
			if (Application.isEditor) return;
			DynamicMissionGenerator.Instance.MissionsPage = this;

			LoadMissions();
			WatchMissions();

			Action goBack = (Action)DynamicMissionGenerator.ModSelectorApi["GoBackMethod"];
			SwitchSelectable.OnInteract += () => { goBack(); return false; };

			FolderSelectable.OnInteract += () => { Application.OpenURL($"file://{missionsFolder}"); return false; };

			foreach (Transform button in ContextMenu.transform)
			{
				button.GetComponent<Button>().onClick.AddListener(() => MenuClick(button));
			}
		}

		private void LoadMissions()
		{
			foreach (Mission mission in missions.Values)
				Destroy(mission.Item.gameObject);

			missions = Directory.GetFiles(missionsFolder).ToDictionary(Path.GetFileNameWithoutExtension, file => {
				var name = Path.GetFileNameWithoutExtension(file);
				Mission mission = null;

				var item = Instantiate(MissionListItem);
				item.transform.Find("MissionName").GetComponent<Text>().text = name;
				item.transform.SetParent(MissionList, false);
				item.GetComponent<Button>().onClick.AddListener(() => {
					if (Input.GetKey(KeyCode.LeftShift))
						return;

					SwitchSelectable.OnInteract();
					DynamicMissionGenerator.Instance.InputPage.LoadMission(mission);
				});

				mission = new Mission(name, File.ReadAllText(file), item);
				return mission;
			});

			NoMissionsText.SetActive(missions.Count == 0);
		}

		private void WatchMissions()
		{
			void updateMissions(object _, FileSystemEventArgs __) 
			{
				LoadMissions();
			}

			var watcher = new FileSystemWatcher
			{
				Path = missionsFolder,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
			};
			watcher.Created += updateMissions;
			watcher.Changed += updateMissions;
			watcher.Deleted += updateMissions;
			watcher.EnableRaisingEvents = true;
		}

		private void Update()
		{
			if (Input.GetMouseButtonUp(0))
			{
				PointerEventData pointerData = new PointerEventData(EventSystem.current) {
					position = Input.mousePosition
				};
				
				List<RaycastResult> results = new List<RaycastResult>();
				EventSystem.current.RaycastAll(pointerData, results);
				
				if (!Input.GetKey(KeyCode.LeftShift) || results.Count == 0 || results[0].gameObject.transform.parent.GetComponent<ModuleListItem>() == null)
				{
					ContextMenu.gameObject.SetActive(false);
					return;
				}

				RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasTransform, Input.mousePosition, Camera.main, out Vector2 localPoint);
				ContextMenu.localPosition = localPoint;
				ContextMenu.gameObject.SetActive(true);

				var result = results[0];
				contextMenuMission = missions.First(pair => pair.Value.Item == result.gameObject.transform.parent.GetComponent<ModuleListItem>()).Value;
			}
		}

		private void MenuClick(Transform button)
		{
			switch (button.name)
			{
				case "Rename":
					Prompt.MakePrompt("Rename Mission", contextMenuMission.Name, CanvasTransform, SwitchSelectable.Parent, name => {
						var targetPath = Path.Combine(missionsFolder, name + ".txt");
						if (File.Exists(targetPath))
						{
							Alert.MakeAlert("Mission Exists", "A mission with that name already exists.", CanvasTransform, SwitchSelectable.Parent);
							return;
						}

						File.Move(Path.Combine(missionsFolder, contextMenuMission.Name + ".txt"), targetPath);
						LoadMissions();
					});
					break;
				case "Duplicate":
					Prompt.MakePrompt("New Mission Name", contextMenuMission.Name + " (Copy)", CanvasTransform, SwitchSelectable.Parent, name => {
						var targetPath = Path.Combine(missionsFolder, name + ".txt");
						if (File.Exists(targetPath))
						{
							Alert.MakeAlert("Mission Exists", "A mission with that name already exists.", CanvasTransform, SwitchSelectable.Parent);
							return;
						}

						File.Copy(Path.Combine(missionsFolder, contextMenuMission.Name + ".txt"), targetPath);
						LoadMissions();
					});
					break;
				case "Delete":
					File.Delete(Path.Combine(missionsFolder, contextMenuMission.Name + ".txt"));
					LoadMissions();
					break;
			}
		}
		
		public class Mission
		{
			public string Name;
			public string Content;
			public ModuleListItem Item;

			public Mission(string name, string content, ModuleListItem item)
			{
				Name = name;
				Content = content;
				Item = item;
			}
		}
	}
}
