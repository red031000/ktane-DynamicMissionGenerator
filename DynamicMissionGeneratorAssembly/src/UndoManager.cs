using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DynamicMissionGeneratorAssembly
{
	internal class UndoManager
	{
		private readonly Stack<State> UndoStack = new Stack<State>();
		private readonly Stack<State> RedoStack = new Stack<State>();
		private State buffer;

		public void Track(string text, int cursor)
		{
			if ((UndoStack.Count != 0 && text == UndoStack.Peek().Text) || (buffer != null && buffer.Text == text))
				return;

			if (Input.inputString.All(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character)))
			{
				if (Input.inputString.Any(character => char.IsWhiteSpace(character)))
				{
					TrackBuffer();
				}

				buffer = new State(text, cursor);
			}
			else
			{
				TrackBuffer();
				UndoStack.Push(new State(text, cursor));
			}
		}

		private void TrackBuffer()
		{
			if (buffer != null)
			{
				if (UndoStack.Count == 0 || UndoStack.Peek() != buffer)
					UndoStack.Push(buffer);
				buffer = null;
			}
		}

		public State Undo()
		{
			TrackBuffer();

			if (UndoStack.Count < 2)
				return null;

			RedoStack.Push(UndoStack.Pop());

			return UndoStack.Peek();
		}

		public State Redo()
		{
			if (RedoStack.Count == 0)
				return null;

			TrackBuffer();

			UndoStack.Push(RedoStack.Pop());

			return UndoStack.Peek();
		}

		public class State
		{
			public State(string text, int cursor)
			{
				Text = text;
				CursorPosition = cursor;
			}

			public string Text;
			public int CursorPosition;
		}
	}
}
