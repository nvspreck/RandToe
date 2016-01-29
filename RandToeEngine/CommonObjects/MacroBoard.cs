using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class MacroBoard
    {
        /// <summary>
        /// The current slots for the board.
        /// </summary>
        public sbyte[][] Slots { get; private set; }

        /// <summary>
        /// The current macro board game states
        /// </summary>
        public sbyte[] MacroBoardStates { get; private set; }

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
        //public MicroBoard[][] MicroBoards { get; private set; }


        #region Static Setup

        /// <summary>
        /// Creates a new board and set the round and slots
        /// </summary>
        /// <param name="round"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static MacroBoard CreateNewBoard(int round, sbyte[] fields)
        {
            return new MacroBoard(round, fields);
        }

        /// <summary>
        /// Sets the macro data for the board.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="macroboard"></param>
        public static void AddMacroboardData(MacroBoard board, sbyte[] macroboard)
        {
            // Set the macro board
            board.MacroBoardStates = macroboard;

            // Create the micro boards
           // board.CreateMicroBoards();

            // Set that we are ready
            board.HasAllData = true;
        }

        ///// <summary>
        ///// Creates the micro boards.
        ///// </summary>
        //private void CreateMicroBoards()
        //{
        //    // Create the rows
        //    MicroBoards = new MicroBoard[3][];

        //    // Make the boards
        //    for(int row = 0; row < 3; row++)
        //    {
        //        MicroBoards[row] = new MicroBoard[3];
        //        for (int col = 0; col < 3; col++)
        //        {
        //            //MicroBoards[row][col] = new MicroBoard();
        //        }
        //    }
        //}


        private MacroBoard(int round, sbyte[] fields)
        {
            // Set the round
            Round = round;

            // Set the slots
            Slots = new sbyte[9][];
            int count = 0;       
            for(int y = 0; y < 9; y++)
            {
                for(int x = 0; x < 9; x++)
                {
                    if(Slots[x] == null)
                    {
                        Slots[x] = new sbyte[9];
                    }
                    Slots[x][y] = fields[count];
                    count++;
                }
            }
        }

        private MacroBoard()
        { }

        #endregion

        #region Copy

        public MacroBoard MakeCopy()
        {
            MacroBoard newBoard = new MacroBoard();
            newBoard.Round = this.Round;
            newBoard.HasAllData = this.HasAllData;

            // Copy macro
            newBoard.MacroBoardStates = new sbyte[9];
            Buffer.BlockCopy(this.MacroBoardStates, 0, newBoard.MacroBoardStates, 0, this.MacroBoardStates.Length * sizeof(byte));

            // Copy slots
            newBoard.Slots = new sbyte[9][];
            for(int i =0; i < 9; i++)
            {
                newBoard.Slots[i] = new sbyte[9];
                Buffer.BlockCopy(this.Slots[i], 0, newBoard.Slots[i], 0, this.Slots.Length * sizeof(byte));
            }
            return newBoard;
        }

        #endregion
    }
}
