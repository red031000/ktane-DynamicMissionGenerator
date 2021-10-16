using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class Alert : MonoBehaviour
	{
		public Button Ok;
		public Text Title;
		public Text Message;

		public void OnDestroy()
		{
		}

		public Alert MakeAlert(string title, string text, Transform parent, KMSelectable parentSelectable)
		{
			var alert = Instantiate(gameObject, parent).GetComponent<Alert>();
			alert.SetupAlert(title, text);

			var selectable = alert.Ok.GetComponent<KMSelectable>();
			selectable.Parent = parentSelectable;
			parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = selectable;
			parentSelectable.UpdateChildren();

			return alert;
		}

		private void SetupAlert(string title, string text)
		{
			Title.text = title;
			Message.text = text;
			Ok.onClick.AddListener(closeAlert);
			Ok.GetComponent<KMSelectable>().OnInteract += () => { closeAlert(); return false; };
		}

		private void closeAlert()
		{
			var parentSelectable = Ok.GetComponent<KMSelectable>().Parent;
			if (parentSelectable != null && parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength]?.gameObject == Ok.gameObject)
			{
				parentSelectable.Children[parentSelectable.Children.Length - parentSelectable.ChildRowLength] = null;
				parentSelectable.UpdateChildren();
			}
			Destroy(gameObject);
		}
	}
}
