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
        public MicroBoard[][] MicroBoards { get; private set; }


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
            board.CreateMicroBoards();

            // Set that we are ready
            board.HasAllData = true;
        }

        public bool HasPlayerWon(sbyte playerId)
        {
            //0,1,2
            //3,4,5
            //6,7,8

            if (MacroBoardStates[0] == playerId)
            {
                if (MacroBoardStates[1] == playerId)
                {
                    if (MacroBoardStates[2] == playerId)
                    {
                        return true;
                    }
                }
                if (MacroBoardStates[4] == playerId)
                {
                    if (MacroBoardStates[8] == playerId)
                    {
                        return true;
                    }
                }
                if (MacroBoardStates[3] == playerId)
                {
                    if (MacroBoardStates[6] == playerId)
                    {
                        return true;
                    }
                }
            }

            if (MacroBoardStates[1] == playerId)
            {
                if (MacroBoardStates[4] == playerId)
                {
                    if (MacroBoardStates[7] == playerId)
                    {
                        return true;
                    }
                }
            }

            if (MacroBoardStates[2] == playerId)
            {
                if (MacroBoardStates[5] == playerId)
                {
                    if (MacroBoardStates[8] == playerId)
                    {
                        return true;
                    }
                }
            }

            if (MacroBoardStates[3] == playerId)
            {
                if (MacroBoardStates[4] == playerId)
                {
                    if (MacroBoardStates[5] == playerId)
                    {
                        return true;
                    }
                }
            }

            if (MacroBoardStates[6] == playerId)
            {
                if (MacroBoardStates[7] == playerId)
                {
                    if (MacroBoardStates[8] == playerId)
                    {
                        return true;
                    }
                }
                if (MacroBoardStates[4] == playerId)
                {
                    if (MacroBoardStates[2] == playerId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the micro boards.
        /// </summary>
        private void CreateMicroBoards()
        {
            // Create the rows
            MicroBoards = new MicroBoard[3][];

            // Make the micro boards
            for(int row = 0; row < 3; row++)
            {
                MicroBoards[row] = new MicroBoard[3];
                for (int col = 0; col < 3; col++)
                {
                   MicroBoards[row][col] = MicroBoard.CreateBoard(this, row, col);
                }
            }
        }

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

        #endregion

        #region Micro Board Logic

        /// <summary>
        /// Returns the Micro board for the given macro x and y cords.
        /// </summary>
        /// <param name="macroX"></param>
        /// <param name="macroY"></param>
        public MicroBoard GetMicroBoardForMacroCords(int macroX, int macroY)
        {
            // Fix the offset
            int microY = (int)Math.Floor(macroY / 3.0);
            int microX = (int)Math.Floor(macroX / 3.0);
            return MicroBoards[microX][microY];
        }

        #endregion

        #region Moves Logic

        /// <summary>
        /// Returns all possible moves for this board.
        /// </summary>
        /// <returns></returns>
        public List<PlayerMove> GetAllPossibleMoves()
        {
            // Run through all of the micro boards and gather any moves they have.
            List<PlayerMove> moves = new List<PlayerMove>();
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    moves.AddRange(MicroBoards[x][y].GetPossibleMoves());
                }
            }
            return moves;
        }

        /// <summary>
        /// Makes a move on the board.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MakeMove(sbyte playerId, PlayerMove moves)
        {
            // Make the move
            Slots[moves.MacroX][moves.MacroY] = playerId;

            // Check for a micro win.
            GetMicroBoardForMacroCords(moves.MacroX, moves.MacroY).CheckForWin();

            // Now see where we are being sent.
            int thisMicroY = (int)Math.Floor(moves.MacroY / 3.0);
            int thisMicroX = (int)Math.Floor(moves.MacroX / 3.0);
            int sendToMicroBoardIndex = (int)((moves.MacroX - (thisMicroX * 3)) + (moves.MacroY - (thisMicroY * 3)) * 3);

            // Figure out what our replace value is. If the board we are being sent to is one they can go anywhere, if not they can't.
            sbyte newReplaceValue = (MacroBoardStates[sendToMicroBoardIndex] == 1 || MacroBoardStates[sendToMicroBoardIndex] == 2) ? (sbyte)-1 : (sbyte)0;

            // Loop through the values
            for (int c = 0; c < 9; c++)
            {
                // If the board isn't won
                if (MacroBoardStates[c] != 1 && MacroBoardStates[c] != 2)
                {
                    // If we are at the board we are looking for.
                    if (c == sendToMicroBoardIndex)
                    {
                        MacroBoardStates[c] = -1;
                    }
                    else
                    {
                        // Set the default value
                        MacroBoardStates[c] = newReplaceValue;
                    }
                }
            }
        }

        #endregion
    }
}
