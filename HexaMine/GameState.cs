/*
 * GameState.cs
 * 
 * States HexaMine game can take
 */
using System;

namespace HexaMine
{
	public enum GameState
	{
		/// <summary>
		/// Game is currently placing mines.
		/// </summary>
		PlacingMines,
		/// <summary>
		/// Game is currently running.
		/// </summary>
		Running,
		/// <summary>
		/// Game is won.
		/// </summary>
		Won,
		/// <summary>
		/// Game is lost.
		/// </summary>
		Lost
	}
}

