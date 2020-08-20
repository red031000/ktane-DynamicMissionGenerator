using UnityEngine;

namespace DynamicMissionGeneratorAssembly
{
	[ExecuteInEditMode]
	class UIElement : MonoBehaviour
	{
		public Texture2D Icon;

		private void Awake()
		{
			var propertyBlock = new MaterialPropertyBlock();
			propertyBlock.SetTexture(Shader.PropertyToID("_MainTex"), Icon);
			transform.Find("Icon").GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
		}
	}
}
