using System;
using System.Windows.Forms;
using System.Drawing;

namespace HexaMine
{
	public class Difficulty : Form
	{
		public Difficulty (int w, int h, int m)
		{
			InitializeComponents (w, h, m);
			Success = false;
		}
		
		public int FWidth { get { return (int)width.Value; } }
		public int FHeight { get { return (int)height.Value; } }
		public int Mines { get { return (int)mines.Value; } }
		public bool Success { get; private set; }
		
		private NumericUpDown width, height, mines;
		
		private void InitializeComponents(int w, int h, int m)
		{
			this.Text = "HexaMine";
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.StartPosition = FormStartPosition.CenterParent;
			this.MaximizeBox = false;
			this.Width = 210;
			this.BackColor = SystemColors.Control;
			
			Label text = new Label();
			text.Text = "Please select difficulty.";
			text.AutoSize = true;
			text.Width = 200;
			text.Location = new Point(5, 5);
			text.Parent = this;
			this.Controls.Add (text);
			
			Label widthl = new Label();
			widthl.Text = "Width";
			widthl.AutoSize = false;
			widthl.Width = 50;
			widthl.Location = new Point(5, text.Top + text.Height + 5);
			widthl.Parent = this;
			this.Controls.Add (widthl);
			
			Label heightl = new Label();
			heightl.Text = "Height";
			heightl.AutoSize = false;
			heightl.Width = 50;
			heightl.Location = new Point(5, widthl.Top + widthl.Height + 5);
			heightl.Parent = this;
			this.Controls.Add (heightl);
			
			Label minesl = new Label();
			minesl.Text = "Mines";
			minesl.AutoSize = false;
			minesl.Width = 50;
			minesl.Location = new Point(5, heightl.Top + heightl.Height + 5);
			minesl.Parent = this;
			this.Controls.Add (widthl);
			
			width = new NumericUpDown();
			width.Minimum = 8;
			width.Maximum = 99;
			width.Value = w;
			width.Parent = this;
			this.Controls.Add (width);
			width.Location = new Point(widthl.Left + widthl.Width + 5, widthl.Top);
			width.Width = 50;
			width.ValueChanged += widthChanged;
			
			height = new NumericUpDown();
			height.Minimum = 8;
			height.Maximum = 99;
			height.Value = h;
			height.Parent = this;
			this.Controls.Add (height);
			height.Location = new Point(heightl.Left + heightl.Width + 5, heightl.Top);
			height.Width = 50;
			height.ValueChanged += widthChanged;
			
			mines = new NumericUpDown();
			mines.Minimum = 1;
			mines.Maximum = 99;
			mines.Value = m;
			mines.Parent = this;
			mines.Location = new Point(minesl.Left + minesl.Width + 5, minesl.Top);
			mines.Width = 50;
			this.Controls.Add (mines);
			
			Button start = new Button();
			start.Text = "New game";
			start.Location = new Point(5, minesl.Top + minesl.Height + 5);
			start.Size = new Size(this.ClientSize.Width - 10, 25);
			start.Parent = this;
			start.Click += HandleStartClick;
			this.Controls.Add (start);
			
			Button easy = new Button();
			easy.Text = "10x10x20";
			easy.Location = new Point(this.ClientSize.Width - 5 - easy.Width, width.Top);
			easy.Height = width.Height;
			easy.Parent = this;
			easy.Click += HandleEasyClick;;
			this.Controls.Add (easy);
			
			Button medium = new Button();
			medium.Text = "16x16x40";
			medium.Location = new Point(this.ClientSize.Width - 5 - medium.Width, height.Top);
			medium.Height = height.Height;
			medium.Parent = this;
			medium.Click += HandleMediumClick;;
			this.Controls.Add (medium);
			
			Button hard = new Button();
			hard.Text = "30x16x99";
			hard.Location = new Point(this.ClientSize.Width - 5 - hard.Width, mines.Top);
			hard.Height = height.Height;
			hard.Parent = this;
			hard.Click += HandleHardClick;;
			this.Controls.Add (hard);
			
			this.ClientSize = new Size(this.ClientSize.Width, start.Top + start.Height + 5);
			
			widthChanged(null, new EventArgs());
		}

		void HandleEasyClick (object sender, EventArgs e)
		{
			width.Value = 10;
			height.Value = 10;
			mines.Value = 20;
		}
		
		void HandleMediumClick (object sender, EventArgs e)
		{
			width.Value = 16;
			height.Value = 16;
			mines.Value = 40;
		}
		void HandleHardClick (object sender, EventArgs e)
		{
			width.Value = 22;
			height.Value = 22;
			mines.Value = 80;
		}

		void HandleStartClick (object sender, EventArgs e)
		{
			if (mines.Value >= width.Value * height.Value)
			{
				MessageBox.Show ("Too many mines!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Success = true;
			this.Close();
		}

		void widthChanged (object sender, EventArgs e)
		{
			mines.Maximum = width.Value * height.Value - 1;
		}
	}
}



