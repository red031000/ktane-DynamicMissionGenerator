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

		private KMAudio Audio;

		private Action<string> OnConfirm;

		public Prompt MakePrompt(string title, string defaultValue, Transform parent, KMSelectable parentSelectable, KMAudio audio, Action<string> confirm)
		{
			var prompt = Instantiate(gameObject, parent).GetComponent<Prompt>();
			prompt.SetupPrompt(title, defaultValue, audio, confirm);

			var confirmSelectable = prompt.Confirm.GetComponent<KMSelectable>();
			var cancelSelectable = prompt.Cancel.GetComponent<KMSelectable>();
			confirmSelectable.Parent = cancelSelectable.Parent = parentSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = confirmSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength + 1] = cancelSelectable;
			parentSelectable.UpdateChildren();

			return prompt;
		}

		private void SetupPrompt(string title, string defaultValue, KMAudio audio, Action<string> confirm)
		{
			Title.text = title;
			Input.text = defaultValue;
			Confirm.onClick.AddListener(() => closePrompt(true));
			Confirm.GetComponent<KMSelectable>().OnInteract += () => { closePrompt(true); return false; };
			Cancel.onClick.AddListener(() => closePrompt(false));
			OnConfirm = confirm;
			Cancel.GetComponent<KMSelectable>().OnInteract += () => { closePrompt(false); return false; };
			// Pass KMAudio from the main pages
			Audio = audio;
		}

		// Allow pressing of Return (Enter) to confirm instead of having to click the button.
		// Setting a bool ensures that it does not fire closePrompt every frame that Return is held.
		// (If the input were empty, it would constantly play the strike sound while Return is held.)
		private bool ReturnPressed;
		void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
			{
				if (!ReturnPressed)
				{
					ReturnPressed = true;
					closePrompt(true);
				}
			}
			else if (ReturnPressed) ReturnPressed = false;
		}

		private void closePrompt(bool confirmed)
		{
			if (confirmed)
			{
				// Ensure that the user does not submit an empty string.
				// Use Trim so that a submission of just spaces (such as "   ") does not save.
				string input = Input.text.Trim();
				if (string.IsNullOrEmpty(input))
				{
					Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
					return;
				}
				OnConfirm(input);
			}

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
