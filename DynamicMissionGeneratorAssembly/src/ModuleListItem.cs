using System;

using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class ModuleListItem : MonoBehaviour
	{
		public event EventHandler Click;

		public Text NameText, IDText;

		private new string name;
		public string Name
		{
			get => name;
			set { name = value; NameText.text = value; }
		}

		private string id;
		public string ID
		{
			get => id;
			set { id = value; IDText.text = value; }
		}

		public void OnClick() { Click?.Invoke(this, EventArgs.Empty); }

		public void HighlightName(int startIndex, int length)
			=> NameText.text = name.Insert(startIndex + length, "</color>").Insert(startIndex, "<color=red>");
		public void HighlightID(int startIndex, int length)
			=> IDText.text = id.Insert(startIndex + length, "</color>").Insert(startIndex, "<color=red>");
	}
}
