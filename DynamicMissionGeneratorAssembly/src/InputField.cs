using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicMissionGeneratorAssembly
{
	public class InputField : UnityEngine.UI.InputField, IScrollHandler
	{
		public Scrollbar Scrollbar;

		public event EventHandler Scroll;
		public event EventHandler Submit;
		public event EventHandler<TabPressedEventArgs> TabPressed;

		protected static char[] separators = new[] { ' ', '\t', '\r', '\n', '*', ';', '\'', '"', ',', '+' };
		protected static FieldInfo allowInputField = typeof(UnityEngine.UI.InputField).GetField("m_AllowInput", BindingFlags.NonPublic | BindingFlags.Instance);
		protected static FieldInfo shouldActivateNextUpdateField = typeof(UnityEngine.UI.InputField).GetField("m_ShouldActivateNextUpdate", BindingFlags.NonPublic | BindingFlags.Instance);

		internal UIVertex[] CursorVertices => m_CursorVerts;

		private float clickTime;
		private Vector2 clickPoint;
		private string prevDrawText = "";
		private bool suppressScrollbarChanged;
		private bool setFocusOnMouseUp;
		private readonly UndoManager undoManager = new UndoManager();

		public void Update()
		{
			undoManager.Track(text, caretPosition);

			// Update the scroll bar.
			if (Scrollbar != null && textComponent.text != prevDrawText)
			{
				float size = cachedInputTextGenerator.rectExtents.height;
				int firstLine = 1;
				for (; firstLine < cachedInputTextGenerator.lineCount; ++firstLine)
				{
					if (cachedInputTextGenerator.lines[firstLine].startCharIdx > m_DrawStart) break;
				}
				--firstLine;
				float topY = cachedInputTextGenerator.lines[firstLine].topY;
				int endLine = firstLine + 1;
				for (; endLine < cachedInputTextGenerator.lineCount; ++endLine)
				{
					if (cachedInputTextGenerator.lines[endLine].topY - cachedInputTextGenerator.lines[endLine].height < topY - size) break;
				}
				suppressScrollbarChanged = true;
				Scrollbar.size = (float) (endLine - firstLine) / cachedInputTextGenerator.lineCount;
				Scrollbar.numberOfSteps = cachedInputTextGenerator.lineCount - (endLine - firstLine) + 1;
				Scrollbar.value = firstLine == 0 ? 0 : (float) firstLine / (Scrollbar.numberOfSteps - 1);  // The conditional avoids division by zero here.
				suppressScrollbarChanged = false;
				prevDrawText = textComponent.text;
			}

			if (setFocusOnMouseUp && !Input.GetMouseButton(0))
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
				setFocusOnMouseUp = false;
			}
		}

		public void Scrollbar_ValueChanged(float value)
		{
			if (suppressScrollbarChanged) return;
			float size = cachedInputTextGenerator.rectExtents.size.y;
			int firstLine = (int) Math.Round(value * (Scrollbar.numberOfSteps - 1));
			int endLine = firstLine + 1;
			float y = cachedInputTextGenerator.lines[firstLine].height;
			for (; endLine < cachedInputTextGenerator.lineCount; ++endLine)
			{
				y += cachedInputTextGenerator.lines[endLine].height;
				if (y > size) break;
			}
			m_DrawStart = cachedInputTextGenerator.lines[firstLine].startCharIdx;
			m_DrawEnd = endLine >= cachedInputTextGenerator.lineCount ? cachedInputTextGenerator.characterCountVisible : cachedInputTextGenerator.lines[endLine].startCharIdx - 1;

			// InputField does not allow the caret to be outside the visible area, so we must move it inside the visible area if it is outside.
			if (caretPosition < m_DrawStart) caretPosition = m_DrawStart;
			else if (caretPosition > m_DrawEnd) caretPosition = m_DrawEnd;

			UpdateLabelWithoutResettingViewRange();
			setFocusOnMouseUp |= Input.GetMouseButton(0);
			Scroll?.Invoke(this, EventArgs.Empty);
		}

		public void OnScroll(PointerEventData e)
		{
			if (Scrollbar != null && Scrollbar.numberOfSteps > 1)
			{
				Scrollbar.value -= e.scrollDelta.y / (Scrollbar.numberOfSteps - 1);
				Scroll?.Invoke(this, EventArgs.Empty);
			}
		}

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

		protected void SuppressSelectAll(Action action)
		{
			int prevAnchorPosition = selectionAnchorPosition, prevCaretPosition = selectionFocusPosition, prevDrawStart = m_DrawStart, prevDrawEnd = m_DrawEnd;
			action();
			selectionAnchorPosition = prevAnchorPosition;
			selectionFocusPosition = prevCaretPosition;
			m_DrawStart = prevDrawStart;
			m_DrawEnd = prevDrawEnd;
			UpdateLabelWithoutResettingViewRange();
		}

		protected void UpdateLabelWithoutResettingViewRange()
		{
			if (isFocused)
				UpdateLabel();
			else
			{
				allowInputField.SetValue(this, true);  // This must be hacked to prevent InputField from forcing the draw range to the start of the text.
				UpdateLabel();
				allowInputField.SetValue(this, false);
			}
		}

		protected override void LateUpdate()
		{
			if ((bool) shouldActivateNextUpdateField.GetValue(this)) SuppressSelectAll(base.LateUpdate);
			else base.LateUpdate();
		}
		public override void OnSelect(BaseEventData e) => SuppressSelectAll(() => base.OnSelect(e));
		public override void OnDeselect(BaseEventData e) => SuppressSelectAll(() => base.OnDeselect(e));

		private new EditState KeyPressed(Event e)
		{
			bool onlyControl = (e.control || e.command) && !e.shift && !e.alt;

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
				case KeyCode.Backspace when onlyControl:
					ProcessEvent(Event.KeyboardEvent("^#left"));
					ProcessEvent(Event.KeyboardEvent("backspace"));
					return EditState.Continue;
				case KeyCode.Delete when onlyControl:
					ProcessEvent(Event.KeyboardEvent("^#right"));
					ProcessEvent(Event.KeyboardEvent("backspace"));
					return EditState.Continue;
				case KeyCode.Z when onlyControl:
					UndoManager.State state = undoManager.Undo();
					if (state != null)
					{
						text = state.Text;
						caretPosition = state.CursorPosition;
					}
					return EditState.Continue;
				case KeyCode.Y when onlyControl:
					UndoManager.State state2 = undoManager.Redo();
					if (state2 != null)
					{
						text = state2.Text;
						caretPosition = state2.CursorPosition;
					}
					return EditState.Continue;
				default:
					switch (e.character)
					{
						case '\t':
							var e2 = new TabPressedEventArgs();
							TabPressed?.Invoke(this, e2);
							if (e2.SuppressKeyPress) return EditState.Continue;
							goto default;
						case '\n': case '\r':
							if (e.control || e.command)
							{
								Submit?.Invoke(this, EventArgs.Empty);
								return EditState.Finish;
							}
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

		public class TabPressedEventArgs : EventArgs
		{
			public bool SuppressKeyPress { get; set; }
		}
	}
}
