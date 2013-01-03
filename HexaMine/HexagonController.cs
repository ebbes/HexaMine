/*
 * HexagonController.cs
 * 
 * HexaMine main logic class
 * Name HexagonController is somehow misleading as this contains all game logic.
 */
using System;
using System.Threading;

namespace HexaMine
{
	public class HexagonController
	{
		public HexagonController (int width, int height, int mines)
		{
			NewGame (width, height, mines);
		}
		
		private Hexagon[,] Hexagons;
		
		public int Width { get; private set; }
		public int Height { get; private set; }
		
		public int MinesCount { get; private set; }
		public int MarkedMinesCount { get; private set; }
		private int ClosedHexagonsCount;
		public int ElapsedSeconds { get; private set; }
		
		private Timer ElapsedSecondsTimer;
		
		public event EventHandler GameWon;
		public event EventHandler GameLost;
		public event EventHandler ElapsedSecondsChanged;
		
		public GameState CurrentGameState { get; private set; }
		
		public void NewGame(int width, int height, int mines)
		{
			StopTimer ();
			
			CurrentGameState = GameState.PlacingMines;
			
			Width = width;
			Height = height;
			MinesCount = mines;
			ClosedHexagonsCount = width * height;
			MarkedMinesCount = 0;
			
			Hexagons = new Hexagon[width, height];
			
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Hexagons[x, y] = new Hexagon(x, y);
				}
			}
			
			Thread PlaceMinesThread = new Thread(PlaceMines);
			PlaceMinesThread.Start ();

		}
		
		public void Restart()
		{
			StopTimer ();
			
			ElapsedSeconds = 0;
			ClosedHexagonsCount = Width * Height;
			MarkedMinesCount = 0;
			
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					Hexagons[x, y].State = HexagonState.Closed;
					Hexagons[x, y].SurroundingMines = null;
					Hexagons[x, y].Pressed = false;
				}
			}
			
			CurrentGameState = GameState.Running;
		}
		
		private void PlaceMinesFinished()
		{
			CurrentGameState = GameState.Running;
			ElapsedSeconds = 0;
			ElapsedSecondsTimer = null;	
		}
		
		private void StartTimer()
		{
			if (ElapsedSecondsTimer == null)
			{
				ElapsedSecondsTimer = new Timer(TimerTick, null, 1000, 1000);
			}
		}
		
		private void StopTimer()
		{
			if (ElapsedSecondsTimer != null)
			{
				ElapsedSecondsTimer.Change (Timeout.Infinite, 1000);
				ElapsedSecondsTimer = null;
			}
		}
		
		private void TimerTick(object state)
		{
			ElapsedSeconds++;
			if (ElapsedSecondsChanged != null)
				ElapsedSecondsChanged(null, new EventArgs());
			if (ElapsedSeconds > 999)
			{
				StopTimer ();
				CurrentGameState = GameState.Lost;
				RevealMines ();
				if (GameLost != null) GameLost(this, new EventArgs());
			}
		}
		
		private void PlaceMines()
		{
			//Currently no good implementation. Painfully slow with many mines
			Random r = new Random(DateTime.Now.Millisecond);
			for (int i = 0; i < MinesCount; )
			{
				int x = r.Next (0, Width);
				int y = r.Next (0, Height);
				
				if (!Hexagons[x, y].IsMine)
				{
					Hexagons[x, y].IsMine = true;
					i++;
				}
			}
			PlaceMinesFinished ();
		}
		
		public Hexagon GetHexagon(int x, int y)
		{
			try
			{
				return Hexagons[x, y];
			}
			catch (IndexOutOfRangeException)
			{
				return new Hexagon(-1, -1);
			}
		}
		
		public void HexagonHover(int x, int y)
		{
			if (x < Width && y < Height && x >= 0 && y >= 0)
			{
				HexagonHover (Hexagons[x, y]);
			}
			else
			{
				HexagonHover (null);
			}
		}
		public void HexagonHover(Hexagon h)
		{
			//Might be slow with big fields. Change implementation somehow?
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					Hexagons[x, y].Hovered = false;
				}
			}
			
			if (h == null)
				return;
			
			h.Hovered = true;
			Hexagon i;
			
			i = TopLeft (h);
			if (i != null) i.Hovered = true;
			i = TopRight (h);
			if (i != null) i.Hovered = true;
			i = Right (h);
			if (i != null) i.Hovered = true;
			i = BottomRight (h);
			if (i != null) i.Hovered = true;
			i = BottomLeft (h);
			if (i != null) i.Hovered = true;
			i = Left (h);
			if (i != null) i.Hovered = true;
		}
		
		public void HexagonClick(Hexagon h)
		{
			if (CurrentGameState != GameState.Running) return;
			
			StartTimer ();
			
			//Don't open already open fields and ignore clicks that happened by accident
			if (h.State == HexagonState.Marked || h.State == HexagonState.Unknown || h.State == HexagonState.Opened) return;
			h.State = HexagonState.Opened;
			ClosedHexagonsCount--;
			
			if (h.IsMine)
			{
				StopTimer ();
				h.State = HexagonState.OpenedMine;
				CurrentGameState = GameState.Lost;
				RevealMines ();
				if (GameLost != null) GameLost(this, new EventArgs());
				return;
			}
			
			h.SurroundingMines = GetSurroundingMines (h);
			
			if (h.SurroundingMines == 0)
			{
				//Open all empty fields recursively...
				OpenHexagonsUntilMine (h);
			}
			
			//Player won the game
			if (ClosedHexagonsCount == MinesCount)
			{
				StopTimer ();
				CurrentGameState = GameState.Won;
				if (GameWon != null) GameWon(this, new EventArgs());
			}
		}
		public void HexagonClick(int x, int y)
		{
			HexagonClick (Hexagons[x, y]);
		}
		
		private void RevealMines()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					Hexagon h = Hexagons[x, y];
					switch (h.State)
					{
					case HexagonState.Closed:
					case HexagonState.Unknown:
						if (h.IsMine)
							h.State = HexagonState.NotMarkedButMine;
						break;
					case HexagonState.Marked:
						if (!h.IsMine)
							h.State = HexagonState.MarkedWrong;
						break;
					}
				}
			}
		}
		
		public void HexagonRightClick(Hexagon h)
		{
			if (h.State == HexagonState.Opened || CurrentGameState != GameState.Running) return;
			
			StartTimer ();
			
			//Other cases only happen when game has ended
			switch (h.State)
			{
			case HexagonState.Opened:
				return;
			case HexagonState.Closed:
				h.State = HexagonState.Marked;
				MarkedMinesCount++;
				return;
			case HexagonState.Marked:
				h.State = HexagonState.Unknown;
				MarkedMinesCount--;
				return;
			case HexagonState.Unknown:
				h.State = HexagonState.Closed;
				return;
			}
		}
		public void HexagonRightClick(int x, int y)
		{
			HexagonRightClick (Hexagons[x, y]);
		}
		
		private void OpenHexagonsUntilMine(Hexagon h)
		{
			//Opens all fields until a mine was found.
			//Convenience-function removes code-duplication
			Hexagon i;
			
			i = TopLeft (h);
			OpenHexagonUntilMine_Convenience(i);
			i = TopRight (h);
			OpenHexagonUntilMine_Convenience(i);
			i = Right (h);
			OpenHexagonUntilMine_Convenience(i);
			i = BottomRight (h);
			OpenHexagonUntilMine_Convenience(i);
			i = BottomLeft (h);
			OpenHexagonUntilMine_Convenience(i);
			i = Left (h);
			OpenHexagonUntilMine_Convenience(i);
		}
		
		private void OpenHexagonUntilMine_Convenience(Hexagon i)
		{
			if (i != null) //null means: Field does not exist
			{
				if (!i.IsMine && i.State != HexagonState.Opened)
				{
					i.SurroundingMines = GetSurroundingMines (i);
					//Remove marked mine. Was wrong btw.
					if (i.State == HexagonState.Marked) MarkedMinesCount++;
					i.State = HexagonState.Opened;
					ClosedHexagonsCount--;
					if (i.SurroundingMines == 0)
						OpenHexagonsUntilMine (i);
				}
			}
		}
		
		public int GetSurroundingMines(Hexagon hex)
		{
			return GetSurroundingMines (hex.X, hex.Y);
		}
		public int GetSurroundingMines(int x, int y)
		{
			int m = 0;
			
			Hexagon h = TopLeft (x, y);
			if (h != null && h.IsMine) m++;
			h = TopRight (x, y);
			if (h != null && h.IsMine) m++;
			h = Right (x, y);
			if (h != null && h.IsMine) m++;
			h = BottomRight (x, y);
			if (h != null && h.IsMine) m++;
			h = BottomLeft (x, y);
			if (h != null && h.IsMine) m++;
			h = Left (x, y);
			if (h != null && h.IsMine) m++;
			
			return m;
		}
		
		#region Procedures to get neighbors
		public Hexagon TopLeft(Hexagon hex)
		{
			return TopLeft (hex.X, hex.Y);
		}
		public Hexagon TopLeft(int x, int y)
		{
			if (!TopLeftExists (x, y)) return null;
			if (y % 2 == 0)
				//Even row
				return Hexagons[x - 1, y - 1];
			else
				//Odd row
				return Hexagons[x, y - 1];
		}
		
		public Hexagon TopRight(Hexagon hex)
		{
			return TopRight (hex.X, hex.Y);
		}
		public Hexagon TopRight(int x, int y)
		{
			if (!TopRightExists (x, y)) return null;
			if (y % 2 == 0)
				//Even row
				return Hexagons[x, y - 1];
			else
				//Odd row
				return Hexagons[x + 1, y - 1];
		}
		
		public Hexagon Right(Hexagon hex)
		{
			return Right (hex.X, hex.Y);
		}
		public Hexagon Right(int x, int y)
		{
			if (!RightExists (x, y)) return null;
			return Hexagons[x + 1, y];
		}
		
		public Hexagon BottomRight(Hexagon hex)
		{
			return BottomRight (hex.X, hex.Y);
		}
		public Hexagon BottomRight(int x, int y)
		{
			if (!BottomRightExists(x, y)) return null;
			if (y % 2 == 0)
				//Even row
				return Hexagons[x, y + 1];
			else
				//Odd row
				return Hexagons[x + 1, y + 1];
		}
		
		public Hexagon BottomLeft(Hexagon hex)
		{
			return BottomLeft (hex.X, hex.Y);
		}
		public Hexagon BottomLeft(int x, int y)
		{
			if (!BottomLeftExists(x, y)) return null;
			if (y % 2 == 0)
				//Even row
				return Hexagons[x - 1, y + 1];
			else
				//Odd row
				return Hexagons[x, y + 1];
		}
		
		public Hexagon Left(Hexagon hex)
		{
			return Left (hex.X, hex.Y);
		}
		public Hexagon Left(int x, int y)
		{
			if (!LeftExists(x, y)) return null;
			return Hexagons[x - 1, y];
		}
		
		public bool TopLeftExists(Hexagon hex)
		{
			return TopLeftExists (hex.X, hex.Y);
		}
		public bool TopLeftExists(int x, int y)
		{
			//No TopLeft in row 0
			if (y == 0) return false;
			//If column 0 and even row, then no TopLeft
			if (x == 0 && y % 2 == 0) return false;
			return true;
		}
		
		public bool TopRightExists(Hexagon hex)
		{
			return TopRightExists (hex.X, hex.Y);
		}
		public bool TopRightExists(int x, int y)
		{
			//Logic similar to TopLeftExists(intx, int y) for all following bools
			if (y == 0) return false;
			if (x == Width - 1 && y % 2 == 1) return false;
			return true;
		}
		
		public bool RightExists(Hexagon hex)
		{
			return RightExists (hex.X, hex.Y);
		}
		public bool RightExists(int x, int y)
		{
			return x != Width - 1;
		}
		
		public bool BottomRightExists(Hexagon hex)
		{
			return BottomRightExists (hex.X, hex.Y);
		}
		public bool BottomRightExists(int x, int y)
		{
			if (y == Height - 1) return false;
			if (x == Width - 1 && y % 2 == 1) return false;
			return true;
		}
		
		public bool BottomLeftExists(Hexagon hex)
		{
			return BottomLeftExists (hex.X, hex.Y);
		}
		public bool BottomLeftExists(int x, int y)
		{
			if (y == Height - 1) return false;
			if (x == 0 && y % 2 == 0) return false;
			return true;
		}
		
		public bool LeftExists(Hexagon hex)
		{
			return LeftExists (hex.X, hex.Y);
		}
		public bool LeftExists(int x, int y)
		{
			return x != 0;
		}
		#endregion
	}
}

