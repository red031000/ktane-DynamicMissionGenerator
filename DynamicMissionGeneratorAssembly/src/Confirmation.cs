using System;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class Confirmation : MonoBehaviour
	{
		public Button Confirm;
		public Button Cancel;
		public Text Title;
		public Text Message;

		private Action OnConfirm;

		public Confirmation MakeConfirmation(string title, string message, Transform parent, KMSelectable parentSelectable, Action confirm)
		{
			var confirmation = Instantiate(gameObject, parent).GetComponent<Confirmation>();
			confirmation.SetupConfirmation(title, message, confirm);

			var confirmSelectable = confirmation.Confirm.GetComponent<KMSelectable>();
			var cancelSelectable = confirmation.Cancel.GetComponent<KMSelectable>();
			confirmSelectable.Parent = cancelSelectable.Parent = parentSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = confirmSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength + 1] = cancelSelectable;
			parentSelectable.UpdateChildren();

			return confirmation;
		}

		private void SetupConfirmation(string title, string message, Action confirm)
		{
			Title.text = title;
			Message.text = message;
			Confirm.onClick.AddListener(() => closeConfirmation(true));
			Confirm.GetComponent<KMSelectable>().OnInteract += () => { closeConfirmation(true); return false; };
			Cancel.onClick.AddListener(() => closeConfirmation(false));
			OnConfirm = confirm;
			Cancel.GetComponent<KMSelectable>().OnInteract += () => { closeConfirmation(false); return false; };
		}

		private void closeConfirmation(bool confirmed)
		{
			if (confirmed)
				OnConfirm();

			var parentSelectable = Confirm.GetComponent<KMSelectable>().Parent;
			if (parentSelectable != null && parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength]?.gameObject == Confirm.gameObject)
			{
				parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = null;
				parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength + 1] = null;
				parentSelectable.UpdateChildren();
			}
			Destroy(gameObject);
		}
	}
}
