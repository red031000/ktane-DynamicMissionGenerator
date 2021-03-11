using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class DynamicMissionGenerator : MonoBehaviour
	{
		public KMSelectable MissionCreationPagePrefab;
		public KMSelectable MissionsPagePrefab;
		public Texture2D ModSelectorIcon;

		[HideInInspector]
		public MissionInputPage InputPage;
		[HideInInspector]
		public MissionsPage MissionsPage;

		public static IDictionary<string, object> ModSelectorApi;
		public static DynamicMissionGenerator Instance;
		public static string MissionsFolder => Path.Combine(Application.persistentDataPath, "DMGMissions");

		internal int? prevRuleSeed;

		private void Start()
		{
			Instance = this;

			StartCoroutine(FindModSelector());

			Directory.CreateDirectory(MissionsFolder);
			GetComponent<KMGameInfo>().OnStateChange += state =>
			{
				if (state == KMGameInfo.State.Setup)
				{
					if (prevRuleSeed.HasValue)
					{
						var obj = GameObject.Find("VanillaRuleModifierProperties");
						var dic = obj?.GetComponent<IDictionary<string, object>>();
						if (dic != null) dic["RuleSeed"] = new object[] { prevRuleSeed, true };
						prevRuleSeed = null;
					}
				}
			};
		}

		private IEnumerator FindModSelector()
		{
			while (true)
			{
				GameObject modSelectorObject = GameObject.Find("ModSelector_Info");
				if (modSelectorObject != null)
				{
					ModSelectorApi = modSelectorObject.GetComponent<IDictionary<string, object>>();
					RegisterService();
					yield break;
				}

				yield return null;
			}
		}

		private void RegisterService()
		{
			Action<KMSelectable> addPageMethod = (Action<KMSelectable>)ModSelectorApi["AddPageMethod"];
			addPageMethod(MissionCreationPagePrefab);
			addPageMethod(MissionsPagePrefab);

			Action<string, KMSelectable, Texture2D> addHomePageMethod = (Action<string, KMSelectable, Texture2D>)ModSelectorApi["AddHomePageMethod"];
			addHomePageMethod("Dynamic Mission Generator", MissionCreationPagePrefab, ModSelectorIcon);
		}
	}
}
