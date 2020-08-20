using UnityEngine;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class Alert : MonoBehaviour
	{
		public Button Ok;
		public Text Title;
		public Text Message;

		public Alert MakeAlert(string title, string text, Transform parent)
		{
			var alert = Instantiate(gameObject, parent).GetComponent<Alert>();
			alert.SetupAlert(title, text);

			return alert;
		}

		private void SetupAlert(string title, string text)
		{
			Title.text = title;
			Message.text = text;
			Ok.onClick.AddListener(() => Destroy(gameObject));
		}
	}
}
