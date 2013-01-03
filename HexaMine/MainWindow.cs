/*
 * MainWindow.cs
 * 
 * HexaMine main window
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HexaMine
{
	public class MainWindow : Form
	{
		private PictureBox DrawingArea;
		private HexagonController Game;
		
		//points defining a hexagon
		private PointF[] HexagonPoints;
		
		//pre-calculated drawing values
		private float MarkerBaseWidth;
		private float MarkerBaseHeight;
		private float MarkerPoleWidth;
		private float MarkerPoleHeight;
		private float MarkerFlagWidth;
		private float MarkerFlagHalfHeight;
		private PointF[] MarkerFlagPoints;
		private float MineWidth;
		
		//x-width of a hexagon
		private float HexagonWidth;
		//"height of the triangle on top of the hexagon's base rectangle"
		private float HexagonTriangleHeight;
		//length of a hexagon side
		private float HexagonSideLength;
		
		//Top left point of game area.
		private PointF FieldOffset;
		//Bottom border width
		private float FieldBottomOffset;
		//Right border width
		private float FieldRightOffset;
		//Point used to center hexagon area
		private PointF CenterOffset;
		
        //Stupid smiley in the middle of the screen
		private RectangleF SmileyRect;
		private bool SmileyNeutral = false;
		private bool MouseDownOnSmiley = false;
		
		//Last clicked hexagon
		private Point LastHexagon;
		
		//60° in Radians
		private const double SixtyDegrees = 60 * Math.PI * 2 / 360;
		
		public MainWindow ()
		{
			this.Menu = new MainMenu();
			MenuItem game = new MenuItem("&Game");
			this.Menu.MenuItems.Add (game);
			game.MenuItems.Add ("&New game", (sender, e) => NewGame ());
			game.MenuItems.Add ("&Restart game", (sender, e) => Restart ());
			game.MenuItems.Add ("-");
			game.MenuItems.Add ("&Select difficulty", (sender, e) => SelectDifficulty());
			game.MenuItems.Add ("-");
			game.MenuItems.Add ("&Quit", (sender, e) => this.Close ());
			
			this.Text = "HexaMine";
			this.StartPosition = FormStartPosition.CenterScreen;
			FieldOffset = new PointF(10, 50);
			FieldBottomOffset = 10f;
			FieldRightOffset = 10f;
			InitializeComponent ();
			Game = new HexagonController(10, 10, 20);
			GameResize (null, new EventArgs());
			Game.GameWon += (sender, e) => DrawingArea.Invalidate();
			Game.GameLost += (sender, e) => DrawingArea.Invalidate();
			Game.ElapsedSecondsChanged += (sender, e) => DrawingArea.Invalidate ();
		}
		
		private void Restart()
		{
			Game.Restart ();
			DrawingArea.Invalidate ();
		}
		
		private void NewGame()
		{
			Game.NewGame (Game.Width, Game.Height, Game.MinesCount);
			DrawingArea.Invalidate ();
		}
		
		private void SelectDifficulty()
		{
			Difficulty dif = new Difficulty(Game.Width, Game.Height, Game.MinesCount);
			dif.ShowDialog ();
			if (dif.Success)
			{
				Game.NewGame (dif.FWidth, dif.FHeight, dif.Mines);
				GameResize (null, null);
				DrawingArea.Invalidate ();
			}
		}
		
		private void InitializeComponent()
		{
			this.ClientSize = new Size(530, 500);
			this.MinimumSize = this.Size;
            this.Resize += (sender, e) => DrawingArea.Invalidate();
			DrawingArea = new PictureBox();
			DrawingArea.Parent = this;
			DrawingArea.Dock = DockStyle.Fill;
			DrawingArea.BackColor = Color.White;
			DrawingArea.Paint += PaintGame;
			DrawingArea.MouseMove += GameMouseMove;
			DrawingArea.MouseUp += GameClick;
			DrawingArea.MouseDown += GameMouseDown;
			DrawingArea.Resize += GameResize;

			this.Controls.Add (DrawingArea);
		}

		void GameResize (object sender, EventArgs e)
		{
			//Get the real width we can use
			float FieldWidth = DrawingArea.Width - FieldOffset.X - FieldRightOffset;
			float FieldHeight = DrawingArea.Height - FieldOffset.Y - FieldBottomOffset;
			
			//Calculate the maximum width of the hexagons
			//Warning: long formulas
			float maxHeight = (float)FieldHeight / (0.5f * (float)Game.Height / (float)Math.Sin(SixtyDegrees) + 0.5f * (float)(Game.Height + 1) / (float)Math.Tan(SixtyDegrees));
			float maxWidth = (float)FieldWidth / ((float)Game.Width + 0.5f);

			//Maximum width of the hexagons is minimum of both values
			HexagonWidth = Math.Min (maxHeight, maxWidth);
			
			//Define a hexagon polygon that will be used for drawing. Point origin doesn't matter as we will use a translation matrix for drawing.
			HexagonPoints = new PointF[6];
			
			HexagonSideLength = (float)HexagonWidth / 2 / (float)Math.Sin (SixtyDegrees);
			HexagonTriangleHeight = HexagonSideLength * (float)Math.Cos (SixtyDegrees);
			
			HexagonPoints[0] = new PointF(0, HexagonTriangleHeight);
			HexagonPoints[1] = new PointF(HexagonWidth / 2, 0);
			HexagonPoints[2] = new PointF(HexagonWidth, HexagonTriangleHeight);
			HexagonPoints[3] = new PointF(HexagonWidth, HexagonTriangleHeight + HexagonSideLength);
			HexagonPoints[4] = new PointF(HexagonWidth / 2, HexagonSideLength + 2 * HexagonTriangleHeight);
			HexagonPoints[5] = new PointF(0, HexagonTriangleHeight + HexagonSideLength);
			
			//Center game area on drawing area
			float ResultingWidth = (float)Game.Width * HexagonWidth + 0.5f * HexagonWidth;
			float ResultingHeight = (float)Game.Height * (HexagonTriangleHeight + HexagonSideLength) + HexagonTriangleHeight;
			
			CenterOffset = new PointF((FieldWidth - ResultingWidth) / 2, (FieldHeight - ResultingHeight) / 2);
			
			//Pre-calculate values for flag markers
			
			//base
			MarkerBaseWidth = 0.5f * HexagonWidth;
			MarkerBaseHeight = 0.2f * HexagonSideLength;
			//flagpole
			MarkerPoleWidth = 0.1f * HexagonWidth;
			MarkerPoleHeight = HexagonSideLength;
			//flag
			MarkerFlagWidth = 0.4f * HexagonWidth;
			MarkerFlagHalfHeight = 0.3f * HexagonSideLength;
			MarkerFlagPoints = new PointF[3];
			MarkerFlagPoints[0] = new PointF(HexagonWidth / 2, HexagonTriangleHeight);
			MarkerFlagPoints[1] = new PointF(HexagonWidth / 2 - MarkerFlagWidth, HexagonTriangleHeight + MarkerFlagHalfHeight);
			MarkerFlagPoints[2] = new PointF(HexagonWidth / 2, HexagonTriangleHeight + 2 * MarkerFlagHalfHeight);
			
			//Pre-calculate values for mines
			MineWidth = 0.75f * HexagonSideLength;
			
			//Pre-calculate Smiley rectangle
			float SmileyWidth = FieldOffset.Y - 10f;
			SmileyRect = new RectangleF((float)this.ClientSize.Width / 2 - SmileyWidth / 2, 5, SmileyWidth, SmileyWidth);
		}

		void GameClick (object sender, MouseEventArgs e)
		{
			SmileyNeutral = false;
			
			if (MouseDownOnSmiley && new RectangleF((float)e.X, (float)e.Y, 1, 1).IntersectsWith(SmileyRect))
			{
				SmileyNeutral = false;
				Game.NewGame (Game.Width, Game.Height, Game.MinesCount);
				LastHexagon = Point.Empty;
				DrawingArea.Invalidate ();
				return;
			}
			
			Point p = GetCoordinatesOfHexagon(e);
			
			int x = p.X;
			int y = p.Y;
			
			if (x > Game.Width - 1 || y > Game.Height - 1 || x < 0 || y < 0) return;
			
			if (LastHexagon == p)
			{
				if (e.Button == MouseButtons.Right)
					Game.HexagonRightClick (x, y);
				else if (e.Button == MouseButtons.Left)
					Game.HexagonClick (x, y);
			}
			Game.GetHexagon (p.X, p.Y).Pressed = false;
			LastHexagon = Point.Empty;
			DrawingArea.Invalidate ();
		}

		void GameMouseDown (object sender, MouseEventArgs e)
		{
			if (new RectangleF((float)e.X, (float)e.Y, 1, 1).IntersectsWith(SmileyRect))
			{
				if (e.Button != MouseButtons.Left)
				{
					SmileyNeutral = false;
					return;
				}
				SmileyNeutral = true;
				MouseDownOnSmiley = true;
				DrawingArea.Invalidate ();
				return;
			}
			MouseDownOnSmiley = false;
			LastHexagon = GetCoordinatesOfHexagon (e);
			
			if (LastHexagon.X >= 0 && LastHexagon.Y >= 0 && Game.CurrentGameState == GameState.Running)
			{
				SmileyNeutral = e.Button != MouseButtons.Right && Game.GetHexagon (LastHexagon.X, LastHexagon.Y).State == HexagonState.Closed;
				Game.GetHexagon (LastHexagon.X, LastHexagon.Y).Pressed = true;
				
				DrawingArea.Invalidate ();
			}
		}
		
		Point GetCoordinatesOfHexagon(MouseEventArgs e)
		{
			bool found = false;
			float MouseX, MouseY;
			int x = 0;
			int y = 0;
			
			//Loop might be slow, especially on large fields, so we'll only check polygons that might be hovered.
			int minX = (int)Math.Max (0, (e.X - FieldOffset.X - CenterOffset.X) / HexagonWidth - 1);
			int minY = (int)Math.Max (0, (e.Y - FieldOffset.Y - CenterOffset.Y) / (HexagonSideLength + HexagonTriangleHeight) - 1);
			
			int maxX = (int)Math.Min((float)Game.Width, minX + 2);
			int maxY = (int)Math.Min ((float)Game.Height, minY + 2);
			for (x = minX; x < maxX; x++)
			{
				for (y = minY; y < maxY; y++)
				{
					if (y % 2 == 1)
						MouseX = e.X - FieldOffset.X - CenterOffset.X - x * HexagonWidth - HexagonWidth / 2;
					else
						MouseX = e.X - FieldOffset.X - CenterOffset.X - x * HexagonWidth;
					MouseY = e.Y - FieldOffset.Y - CenterOffset.Y - y * (HexagonSideLength + HexagonTriangleHeight);
					found = IsPointInPolygon (HexagonPoints, new PointF(MouseX, MouseY));
					if (found) break;
				}
			
				if (found) break;
			}
			return found ? new Point((int)x, (int)y) : new Point(-1, -1); 
		}
		
		//Code taken from http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/95055cdc-60f8-4c22-8270-ab5f9870270a/
		//slightly modified
		private static bool IsPointInPolygon(PointF[] poly, PointF p)
	    {
		    if (poly.Length < 3)
	        {
	            return false;
	        }
			
			PointF p1, p2;
			bool inside = false;
	        var oldPoint = new PointF(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

	        for (int i = 0; i < poly.Length; i++)
	        {
	            var newPoint = new PointF(poly[i].X, poly[i].Y);
		        
				if (newPoint.X > oldPoint.X)
	            {
	                p1 = oldPoint;
	                p2 = newPoint;
	            }
	            else
	            {
	                p1 = newPoint;
	                p2 = oldPoint;
	            }

	            if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && (p.Y - (long)p1.Y) * (p2.X - p1.X) < (p2.Y - (long)p1.Y) * (p.X - p1.X))
	            {
	                inside = !inside;
	            }

	            oldPoint = newPoint;
	        }
	        return inside;
	    }

		
		void GameMouseMove (object sender, MouseEventArgs e)
		{
			Point p = GetCoordinatesOfHexagon (e);
			
			if (p != LastHexagon)
				Game.GetHexagon (LastHexagon.X, LastHexagon.Y).Pressed = false;
			
			Game.HexagonHover (p.X, p.Y);
			
			DrawingArea.Invalidate ();
		}

		void PaintGame (object sender, PaintEventArgs e)
		{
			e.Graphics.Clear (SystemColors.Control);
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			e.Graphics.FillRectangle (Brushes.Gray, FieldOffset.X + CenterOffset.X + 1, FieldOffset.Y + CenterOffset.Y + 1, (int)((float)Game.Width * HexagonWidth + 0.5f * HexagonWidth) + 1, (int)((float)Game.Height * (HexagonTriangleHeight + HexagonSideLength) + HexagonTriangleHeight ) + 1);
			e.Graphics.DrawRectangle (new Pen(Brushes.Black, 3f), FieldOffset.X + CenterOffset.X, FieldOffset.Y + CenterOffset.Y, (int)((float)Game.Width * HexagonWidth + 0.5f * HexagonWidth) + 1, (int)((float)Game.Height * (HexagonTriangleHeight + HexagonSideLength) + HexagonTriangleHeight + 1));
			
			float top = 0;
			for (int y = 0; y < Game.Height; y++)
			{
				float left = 0;
				if (y % 2 == 1)
					left = HexagonWidth / 2;
				for (int x = 0; x < Game.Width; x++)
				{
					Hexagon h = Game.GetHexagon (x, y);
					
					e.Graphics.ResetTransform ();
					e.Graphics.TranslateTransform (left + FieldOffset.X + CenterOffset.X, top + FieldOffset.Y + CenterOffset.Y);
					
					DrawHexagon (h, e.Graphics);
					
					
					left += HexagonWidth;
				}
				
				top += HexagonTriangleHeight + HexagonSideLength;
			}
			
			e.Graphics.ResetTransform ();
			
			if (Game.CurrentGameState == GameState.PlacingMines)
			{
				DrawMessage (e.Graphics, "Placing mines...");
			}
			else if (Game.CurrentGameState == GameState.Lost)
			{
				DrawMessage (e.Graphics, "You lost the game.");
			}
			else if (Game.CurrentGameState == GameState.Won)
			{
				DrawMessage (e.Graphics, "You won the game in " + Game.ElapsedSeconds + " seconds!");
			}

			Font HUDFont = new Font(FontFamily.GenericMonospace, 24f);
			string minesString = (Game.MinesCount - Game.MarkedMinesCount).ToString("D3");
			SizeF HUDSize = e.Graphics.MeasureString(minesString, HUDFont);
			RectangleF HUDRect = new RectangleF(FieldOffset.X + CenterOffset.X, (FieldOffset.Y - 5) / 2 - HUDSize.Height / 2, HUDSize.Width, HUDSize.Height);
			e.Graphics.FillRectangle(Brushes.Black, HUDRect);
			e.Graphics.DrawString(minesString, HUDFont, Brushes.Red, HUDRect);
			
			string secondsString = Game.ElapsedSeconds.ToString ("D3");
			HUDSize = e.Graphics.MeasureString (secondsString, HUDFont);
			HUDRect = new RectangleF((float)this.ClientSize.Width - FieldOffset.X - CenterOffset.X - HUDSize.Width + 2, (FieldOffset.Y - 5) / 2 - HUDSize.Height / 2, HUDSize.Width, HUDSize.Height);
			e.Graphics.FillRectangle (Brushes.Black, HUDRect);
			e.Graphics.DrawString (secondsString, HUDFont, Brushes.Red, HUDRect);
			HUDFont.Dispose ();
			
			DrawSmiley(e.Graphics);
		}
		
		void DrawSmiley(Graphics g)
		{
			g.FillEllipse(Brushes.Yellow, SmileyRect);
			g.DrawEllipse (Pens.Black, SmileyRect);
			
			var state = g.Save ();
			g.TranslateTransform (SmileyRect.X, SmileyRect.Y);
			if (Game.CurrentGameState != GameState.Lost || SmileyNeutral)
			{
				//draw eyes
				g.FillEllipse(Brushes.Black, 0.3f * SmileyRect.Width, 0.3f * SmileyRect.Height, 0.1f * SmileyRect.Width, 0.1f * SmileyRect.Height);
				g.FillEllipse(Brushes.Black, 0.7f * SmileyRect.Width, 0.3f * SmileyRect.Height, -0.1f * SmileyRect.Width, 0.1f * SmileyRect.Height);
				
				//draw mouth
				if (SmileyNeutral)
				{
					g.DrawLine(Pens.Black, 0.2f * SmileyRect.Width, 0.7f * SmileyRect.Height, 0.8f * SmileyRect.Width, 0.7f * SmileyRect.Height);
				}
				else
				{
					g.DrawArc(Pens.Black, 0.2f * SmileyRect.Width, 0.4f * SmileyRect.Height, 0.6f * SmileyRect.Width, 0.4f * SmileyRect.Height, 180, -180);
				}
			}
			else
			{
				//sad mouth
				g.DrawArc(Pens.Black, 0.2f * SmileyRect.Width, 0.6f * SmileyRect.Height, 0.6f * SmileyRect.Width, 0.4f * SmileyRect.Height, -180, 180);
				
				//crosses as eyes
				var state2 = g.Save ();
				g.TranslateTransform (0.35f * SmileyRect.Width, 0.35f * SmileyRect.Height);
				g.RotateTransform (45);
				g.DrawLine (Pens.Black, -0.1f * SmileyRect.Width, 0, 0.1f * SmileyRect.Width, 0);
				g.RotateTransform (90);
				g.DrawLine (Pens.Black, -0.1f * SmileyRect.Width, 0, 0.1f * SmileyRect.Width, 0);
				g.Restore (state2);
				
				g.TranslateTransform (0.65f * SmileyRect.Width, 0.35f * SmileyRect.Height);
				g.RotateTransform (45);
				g.DrawLine (Pens.Black, -0.1f * SmileyRect.Width, 0, 0.1f * SmileyRect.Width, 0);
				g.RotateTransform (90);
				g.DrawLine (Pens.Black, -0.1f * SmileyRect.Width, 0, 0.1f * SmileyRect.Width, 0);
				g.Restore (state2);
			}

			g.Restore (state);
		}
		
		void DrawMessage(Graphics g, string s)
		{
			Font MessageFont = new Font("Helvetica", 22f, FontStyle.Bold);
			SizeF MessageSize = g.MeasureString (s, MessageFont);
			PointF Origin = new PointF((float)DrawingArea.Width / 2 - MessageSize.Width / 2, (float)DrawingArea.Height / 2 - MessageSize.Height / 2);
			g.FillRectangle (new SolidBrush(Color.FromArgb (128, 0, 0, 0)), new RectangleF(Origin, MessageSize));
			g.DrawString (s, MessageFont, Brushes.White, Origin);
			MessageFont.Dispose ();
		}
		
		void DrawHexagon(Hexagon h, Graphics g)
		{
			switch (h.State)
			{
			case HexagonState.Closed:
				g.FillPolygon (Brushes.LightGray, HexagonPoints);
				break;
			case HexagonState.Marked:
				g.FillPolygon (Brushes.LightGray, HexagonPoints);
				DrawMarker (g);
				break;
			case HexagonState.Unknown:
				g.FillPolygon (Brushes.LightGray, HexagonPoints);
				DrawStringCentered (g, "?", Color.Black);
				break;
			case HexagonState.Opened:
				g.FillPolygon (Brushes.Gray, HexagonPoints);
				if (h.SurroundingMines > 0)
					DrawStringCentered (g, h.SurroundingMines.ToString (), GetSurroundingMinesColor ((int)h.SurroundingMines));
				break;
			case HexagonState.OpenedMine:
				g.FillPolygon (Brushes.Red, HexagonPoints);
				DrawMine(g);
				break;
			case HexagonState.NotMarkedButMine:
				g.FillPolygon (Brushes.LightGray, HexagonPoints);
				DrawMine(g);
				break;
			case HexagonState.MarkedWrong:
				g.FillPolygon (Brushes.LightGray, HexagonPoints);
				DrawMarker(g);
				var state = g.Save ();
				DrawCross(g, Brushes.White, true);
				g.Restore (state);
				break;
			}
			if (h.Hovered)
				g.FillPolygon (new SolidBrush(Color.FromArgb (45, 255, 255, 30)), HexagonPoints);
			if (h.Pressed && (h.State == HexagonState.Closed || h.State == HexagonState.Marked || h.State == HexagonState.Unknown))
				g.FillPolygon(new SolidBrush(Color.FromArgb (90,0,0,0)), HexagonPoints);
			g.DrawPolygon (new Pen(Brushes.Black, 2f), HexagonPoints);
		}
		
		private void DrawStringCentered(Graphics g, string s, Color c)
		{
			Font f = new Font("Helvetica", HexagonSideLength * 72 / g.DpiY, FontStyle.Bold);
			SizeF size = g.MeasureString(s, f);
			g.DrawString (s, f, new SolidBrush(c), HexagonWidth / 2 - size.Width / 2,
			              (HexagonSideLength + 2 * HexagonTriangleHeight) / 2 - size.Height / 2);
			f.Dispose ();
		}
		
		private Color GetSurroundingMinesColor(int s)
		{
			switch (s)
			{
			case 1:
				return Color.Blue;
			case 2:
				return Color.Green;
			case 3:
				return Color.Red;
			case 4:
				return Color.DarkBlue;
			case 5:
				return Color.DarkRed;
			case 6:
				return Color.Violet;
			default:
				return Color.Black;
			}
		}
		
		private void DrawMine(Graphics g)
		{
			g.FillEllipse (Brushes.Black, HexagonWidth / 2 - MineWidth / 2, HexagonTriangleHeight + HexagonSideLength / 2 - MineWidth / 2, MineWidth, MineWidth);
			var c = g.Save ();
			DrawCross (g, Brushes.Black, true);
			DrawCross (g, Brushes.Black, false);
			g.Restore (c);
		}
		
		private void DrawMarker(Graphics g)
		{
			g.FillRectangle (Brushes.Black, HexagonWidth / 2 - MarkerBaseWidth / 2, HexagonTriangleHeight + HexagonSideLength - MarkerBaseHeight, MarkerBaseWidth, MarkerBaseHeight);
			g.FillRectangle (Brushes.Black, HexagonWidth / 2 - MarkerPoleWidth / 2, HexagonTriangleHeight + HexagonSideLength - MarkerPoleHeight, MarkerPoleWidth, MarkerPoleHeight);
			g.FillPolygon (Brushes.Red, MarkerFlagPoints);
		}
		
		private void DrawCross(Graphics g, Brush b, bool doTranslate)
		{
			if (doTranslate)
				g.TranslateTransform (HexagonWidth / 2, HexagonTriangleHeight + HexagonSideLength / 2);
			g.RotateTransform (45f);
			g.FillRectangle (b, -HexagonSideLength / 2, -0.1f * HexagonSideLength, HexagonSideLength, 0.2f * HexagonSideLength);
			g.RotateTransform (90f);
			g.FillRectangle (b, -HexagonSideLength / 2, -0.1f * HexagonSideLength, HexagonSideLength, 0.2f * HexagonSideLength);
		}
	}
}

