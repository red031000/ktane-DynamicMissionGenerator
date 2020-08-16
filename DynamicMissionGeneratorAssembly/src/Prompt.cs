using System;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class Prompt : MonoBehaviour
	{
		public Button Confirm;
		public Button Cancel;
		public InputField Input;
		public Text Title;

		private Action<string> OnConfirm;

		public Prompt MakePrompt(string title, string defaultValue, Transform parent, Action<string> confirm)
		{
			var prompt = Instantiate(gameObject, parent).GetComponent<Prompt>();
			prompt.SetupPrompt(title, defaultValue, confirm);

			return prompt;
		}

		private void SetupPrompt(string title, string defaultValue, Action<string> confirm)
		{
			Title.text = title;
			Input.text = defaultValue;
			Confirm.onClick.AddListener(() => closePrompt(true));
			Cancel.onClick.AddListener(() => closePrompt(false));
			OnConfirm = confirm;
		}

		public Prompt MakeAlert(string title, string text, Transform parent)
		{
			var prompt = Instantiate(gameObject, parent).GetComponent<Prompt>();
			prompt.SetupAlert(title, text);

			return prompt;
		}

		private void SetupAlert(string title, string text)
		{
			Title.text = title;
			Confirm.onClick.AddListener(() => closePrompt(true));
			Cancel.onClick.AddListener(() => closePrompt(false));
		}

		private void closePrompt(bool confirmed)
		{
			if (confirmed)
				OnConfirm(Input.text);

			Destroy(gameObject);
		}
	}
}
