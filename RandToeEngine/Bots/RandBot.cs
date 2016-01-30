using RandToeEngine.CommonObjects;
using RandToeEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Bots
{
    /// <summary>
    /// The best bot ever. Random.
    /// </summary>
    class RandBot : IPlayer
    {
        /// <summary>
        /// Called when we should make a move.
        /// </summary>
        /// <param name="playerBase"></param>
        public void MoveRequested(IPlayerBase playerBase)
        {
            // Get all possible moves on the board.
            List<PlayerMove> possibleMoves = playerBase.CurrentBoard.GetAllPossibleMoves();

            // Make random
            Random rand = new Random((int)DateTime.Now.Ticks);

            // Select a move
            PlayerMove selectedMove = possibleMoves[rand.Next(0, possibleMoves.Count)];

            // Play the move
            if(!playerBase.MakeMove(selectedMove))
            {
                PlayerBase.Logger.Log(this, "Randomly selected move invalid, this shouldn't happen", LogLevels.Error);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

        }
    }
}
