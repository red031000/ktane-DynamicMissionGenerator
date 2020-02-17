using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly {
	public class ModuleListItem : MonoBehaviour {
		public event EventHandler Click;

		public Text NameText, IDText;

		private new string name;
		public string Name {
			get => this.name;
			set { this.name = value; this.NameText.text = value; }
		}

		private string id;
		public string ID {
			get => this.id;
			set { this.id = value; this.IDText.text = value; }
		}

		public void OnClick() { this.Click?.Invoke(this, EventArgs.Empty); }

		public void HighlightName(int startIndex, int length)
			=> this.NameText.text = this.name.Insert(startIndex + length, "</color>").Insert(startIndex, "<color=red>");
		public void HighlightID(int startIndex, int length)
			=> this.IDText.text = this.id.Insert(startIndex + length, "</color>").Insert(startIndex, "<color=red>");
	}
}
