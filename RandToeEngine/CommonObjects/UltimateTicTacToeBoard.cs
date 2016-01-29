using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class UltimateTicTacToeBoard
    {
        /// <summary>
        /// The current slots for the board.
        /// </summary>
        public int[][] Slots { get; private set; }

        /// <summary>
        /// The current macro board game states
        /// </summary>
        public int[] MacroBoardStates { get; private set; }

        /// <summary>
        /// The round this board represents
        /// </summary>
        public int Round { get; private set; }

        /// <summary>
        /// Indicates if the board is ready for consumption.
        /// </summary>
        public bool HasAllData { get; private set; } = false;

        /// <summary>
        /// The micro boards that represent this game board.
        /// </summary>
        public MicroBoard[][] MicroBoards { get; private set; }


        #region Static Setup

        /// <summary>
        /// Creates a new board and set the round and slots
        /// </summary>
        /// <param name="round"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static UltimateTicTacToeBoard CreateNewBoard(int round, int[] fields)
        {
            return new UltimateTicTacToeBoard(round, fields);
        }

        /// <summary>
        /// Sets the macro data for the board.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="macroboard"></param>
        public static void AddMacroboardData(UltimateTicTacToeBoard board, int[] macroboard)
        {
            // Set the macro board
            board.MacroBoardStates = macroboard;

            // Create the micro boards
            board.CreateMicroBoards();

            // Set that we are ready
            board.HasAllData = true;
        }

        /// <summary>
        /// Creates the micro boards.
        /// </summary>
        private void CreateMicroBoards()
        {
            // Create the rows
            MicroBoards = new MicroBoard[3][];

            // Make the boards
            for(int row = 0; row < 3; row++)
            {
                MicroBoards[row] = new MicroBoard[3];
                for (int col = 0; col < 3; col++)
                {
                   // MicroBoards[row][col] = new MicroBoard();
                }
            }
        }


        private UltimateTicTacToeBoard(int round, int[] fields)
        {
            // Set the round
            Round = round;

            // Set the slots
            Slots = new int[9][];
            for(int row = 0; row < 9; row++)
            {
                Slots[row] = new int[9];
                Array.ConstrainedCopy(fields, row * 9, Slots[row], 0, 9);
            }
        }

        #endregion
    }
}
