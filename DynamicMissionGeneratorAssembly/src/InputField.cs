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
		protected static char[] separators = new[] { ' ', '\t', '\r', '\n', '*', ';', '\'', '"', ',', '+' };

		internal UIVertex[] CursorVertices => m_CursorVerts;

		private float clickTime;
		private Vector2 clickPoint;

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
			switch (e.keyCode)
			{
				case KeyCode.Home:
					if (e.control || e.command) MoveTextStart(e.shift);
					else
					{
						var startOfLine = caretSelectPositionInternal <= 2 ? 0 : text.LastIndexOf('\n', caretSelectPositionInternal - 2) + 1;
						if (!e.shift) caretPositionInternal = startOfLine;
						caretSelectPositionInternal = startOfLine;
					}
					return EditState.Continue;
				case KeyCode.End:
					if (e.control || e.command) MoveTextEnd(e.shift);
					else
					{
						var endOfLine = caretSelectPositionInternal + 1 >= text.Length ? text.Length : text.IndexOf('\n', caretSelectPositionInternal + 1);
						if (endOfLine < 0) endOfLine = text.Length;
						if (!e.shift) caretPositionInternal = endOfLine;
						caretSelectPositionInternal = endOfLine;
					}
					return EditState.Continue;
				default:
					switch (e.character)
					{
						case '\t':
							return EditState.Continue;
						case '\n': case '\r':
							if (e.control || e.command) return EditState.Finish;
							goto default;
						default:
							return base.KeyPressed(e);
					}
			}
		}

		public override void OnPointerClick(PointerEventData e)
		{
			base.OnPointerClick(e);
			if (e.button == PointerEventData.InputButton.Left)
			{
				if ((Vector2) Input.mousePosition == clickPoint && Time.time - clickTime < 0.5f)
				{
					int left = caretSelectPositionInternal == 0 ? 0 : (text.LastIndexOfAny(separators, caretSelectPositionInternal - 1) + 1);
					int right = text.IndexOfAny(separators, caretSelectPositionInternal);
					if (right < 0) right = text.Length;
					caretPositionInternal = left;
					caretSelectPositionInternal = right;
					UpdateLabel();
					clickTime = 0;
				}
				else clickTime = Time.time;
				clickPoint = Input.mousePosition;
			}
		}

		internal new int GetCharacterIndexFromPosition(Vector2 position) => base.GetCharacterIndexFromPosition(position);
	}
}
