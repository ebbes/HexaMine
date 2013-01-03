/*
 * HexagonState.cs
 * 
 * States a hexagon field can take
 */
using System;

namespace HexaMine
{
	/// <summary>
	/// Hexagon state.
	/// </summary>
	public enum HexagonState
	{
		/// <summary>
		/// Hexagon was not yet opened.
		/// </summary>
		Closed,
		/// <summary>
		/// Hexagon was opened.
		/// </summary>
		Opened,
		/// <summary>
		/// Hexagon was opened, but contains mine. Game lost.
		/// </summary>
		OpenedMine,
		/// <summary>
		/// Hexagon was marked as mine but doesn't contain a mine. After game lost.
		/// </summary>
		MarkedWrong,
		/// <summary>
		/// Hexagon was not marked but contains a mine. After game lost.
		/// </summary>
		NotMarkedButMine,
		/// <summary>
		/// Hexagon was not yet opened and is marked.
		/// </summary>
		Marked,
		/// <summary>
		/// Hexagon was not yet opened and is marked with a question mark.
		/// </summary>
		Unknown
	}
}

