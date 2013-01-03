/*
 * Hexagon.cs
 * 
 * Class containing information about hexagon field.
 * No logic in here, look for HexagonController.cs
 */
using System;

namespace HexaMine
{
	public class Hexagon
	{
		public int X { get; private set; }
		public int Y { get; private set; }
		public bool IsMine { get; set; }
		public HexagonState State { get; set; }
		public int? SurroundingMines { get; set; }
		public bool Hovered { get; set; }
		public bool Pressed { get; set; }
		public Hexagon (int x, int y)
		{
			X = x;
			Y = y;
			State = HexagonState.Closed;
			SurroundingMines = null;
			Hovered = false;
			Pressed = false;
		}
	}
}

