using RandToe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToe
{
    public interface IPlayerBase
    {
        /// <summary>
        /// How much max time is remaining.
        /// </summary>
        int Timebank { get; }

        /// <summary>
        /// How much time is added per move.
        /// </summary>
        int TimeAddedPerMove { get; }

        /// <summary>
        /// A list of the names of the player playing
        /// </summary>
        List<string> Players { get; }

        /// <summary>
        /// Your player name.
        /// </summary>
        string PlayerName { get; }

        /// <summary>
        /// The value that you will use to make moves.
        /// </summary>
        sbyte PlayerId { get; }

        /// <summary>
        /// The current round.
        /// </summary>
        int CurrentRound { get; }

        /// <summary>
        /// The current move
        /// </summary>
        int CurrentMove { get; }

        /// <summary>
        /// The current board.
        /// </summary>
        MacroBoard CurrentBoard { get; }

        /// <summary>
        /// Returns a list of the past moves.
        /// </summary>
        List<PlayerMove> PreviousMoves { get; }


        /// <summary>
        /// A list showing the past player moves.
        /// </summary>
        List<HistoricPlayerMove> HistoricMoves { get; }

        /// <summary>
        /// Called when we should make a move.
        /// </summary>
        /// <returns>true if the move has been made, false if it is invalid.</returns>
        bool MakeMove(PlayerMove move);
    }
}
