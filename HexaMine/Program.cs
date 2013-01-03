/*
 * Program.cs
 * 
 * HexaMine main entry point
 */
using System;
using System.Windows.Forms;
namespace HexaMine
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles ();
			Application.Run (new MainWindow());
		}
	}
}

