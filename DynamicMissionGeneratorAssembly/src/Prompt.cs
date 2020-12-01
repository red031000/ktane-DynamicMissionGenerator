using System;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class Prompt : MonoBehaviour
	{
		public Button Confirm;
		public Button Cancel;
		public UnityEngine.UI.InputField Input;
		public Text Title;

		private Action<string> OnConfirm;

		public Prompt MakePrompt(string title, string defaultValue, Transform parent, KMSelectable parentSelectable, Action<string> confirm)
		{
			var prompt = Instantiate(gameObject, parent).GetComponent<Prompt>();
			prompt.SetupPrompt(title, defaultValue, confirm);

			var confirmSelectable = prompt.Confirm.GetComponent<KMSelectable>();
			var cancelSelectable = prompt.Cancel.GetComponent<KMSelectable>();
			confirmSelectable.Parent = cancelSelectable.Parent = parentSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = confirmSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength + 1] = cancelSelectable;
			parentSelectable.UpdateChildren();

			return prompt;
		}

		private void SetupPrompt(string title, string defaultValue, Action<string> confirm)
		{
			Title.text = title;
			Input.text = defaultValue;
			Confirm.onClick.AddListener(() => closePrompt(true));
			Confirm.GetComponent<KMSelectable>().OnInteract += () => { closePrompt(true); return false; };
			Cancel.onClick.AddListener(() => closePrompt(false));
			OnConfirm = confirm;
			Cancel.GetComponent<KMSelectable>().OnInteract += () => { closePrompt(false); return false; };
		}

		private void closePrompt(bool confirmed)
		{
			if (confirmed)
				OnConfirm(Input.text);

			var parentSelectable = Confirm.GetComponent<KMSelectable>().Parent;
			if (parentSelectable != null && parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength]?.gameObject == Confirm.gameObject)
			{
				// It's possible that another dialog has been created. Do not remove its Selectables if so.
				parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = null;
				parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength + 1] = null;
				parentSelectable.UpdateChildren();
			}
			Destroy(gameObject);
		}
	}
}
