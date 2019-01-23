using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicMissionGeneratorAssembly
{
	public class DynamicMissionGenerator : MonoBehaviour
	{
		public KMSelectable MissionCreationPagePrefab;
		public Texture2D ModSelectorIcon;

		public static IDictionary<string, object> ModSelectorApi;
		private void Start()
		{
			StartCoroutine(FindModSelector());
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

			Action<string, KMSelectable, Texture2D> addHomePageMethod = (Action<string, KMSelectable, Texture2D>)ModSelectorApi["AddHomePageMethod"];
			addHomePageMethod("Dynamic Mission Generator", MissionCreationPagePrefab, ModSelectorIcon);
		}
	}
}
