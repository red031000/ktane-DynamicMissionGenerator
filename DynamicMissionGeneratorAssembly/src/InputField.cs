using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DynamicMissionGeneratorAssembly
{
	public class InputField : UnityEngine.UI.InputField
	{
		internal UIVertex[] CursorVertices => m_CursorVerts;

		public override void OnUpdateSelected(BaseEventData eventData)
		{
			if (isFocused)
			{
				var e = new Event();
				while (Event.PopEvent(e))
				{
					if (e.rawType == EventType.KeyDown)
					{
						KeyPressed(e);
						UpdateLabel();
					}
				}
				eventData.Use();
			}
		}

		private new EditState KeyPressed(Event e)
		{
			switch (e.character)
			{
				case '\t':
					return EditState.Continue;
				case '\n':
					if (e.control || e.command) return EditState.Continue;
					goto default;
				default:
					return base.KeyPressed(e);
			}
		}

		internal new int GetCharacterIndexFromPosition(Vector2 position) => base.GetCharacterIndexFromPosition(position);
	}
}
