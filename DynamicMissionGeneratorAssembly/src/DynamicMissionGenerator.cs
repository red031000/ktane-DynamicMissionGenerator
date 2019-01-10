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

		public KMSelectable RunButton;

		private static IDictionary<string, object> _modSelectorApi;
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
					_modSelectorApi = modSelectorObject.GetComponent<IDictionary<string, object>>();
					RegisterService();
					yield break;
				}

				yield return null;
			}
		}

		private void RegisterService()
		{
			Action<KMSelectable> addPageMethod = (Action<KMSelectable>)_modSelectorApi["AddPageMethod"];
			addPageMethod(MissionCreationPagePrefab);

			Action<string, KMSelectable, Texture2D> addHomePageMethod = (Action<string, KMSelectable, Texture2D>)_modSelectorApi["AddHomePageMethod"];
			addHomePageMethod("Dynamic Mission Generator", MissionCreationPagePrefab, ModSelectorIcon);
		}

		private void Awake()
		{
			RunButton.OnInteract += RunInteract;
		}

		private bool RunInteract()
		{

			return false;
		}
	}
}
