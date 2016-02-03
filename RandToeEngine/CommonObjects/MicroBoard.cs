using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class MicroBoard
    {
        readonly int m_thisBoardRow;
        readonly int m_thisBoardCol;
        readonly MacroBoard m_macroBoard;

        #region Creation

        /// <summary>
        /// Creates a new micro board given the parent macro board and the position.
        /// </summary>
        /// <param name="macroBoard"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static MicroBoard CreateBoard(MacroBoard macroBoard, int row, int col)
        {
            MicroBoard board = new MicroBoard(macroBoard, row, col);
            return board;
        }

        private MicroBoard(MacroBoard macroBoard, int row, int col)
        {
            m_thisBoardRow = row;
            m_thisBoardCol = col;
            m_macroBoard = macroBoard;
        }

        #endregion

        #region General Maths

        /// <summary>
        /// Returns the position of this micro board in the macro board state array.
        /// </summary>
        /// <returns></returns>
        public int GetLinearStateArrayPos()
        {
            return m_thisBoardRow + m_thisBoardCol * 3;
        }


        #endregion

        #region Board States

        /// <summary>
        /// Returns if the board is playable.
        /// </summary>
        public bool IsPlayable
        {
            get
            {
                return m_macroBoard.MacroBoardStates[GetLinearStateArrayPos()] == -1;
            }
        }

        /// <summary>
        /// Returns if the board is won by anyone.
        /// </summary>
        public bool IsWon
        {
            get
            {
                int state = m_macroBoard.MacroBoardStates[GetLinearStateArrayPos()];
                return state != 0 && state != -1;
            }
        }

        /// <summary>
        /// Returns if the the board state.
        /// </summary>
        public int BoardState
        {
            get
            {
                return m_macroBoard.MacroBoardStates[GetLinearStateArrayPos()];
            }
        }

        #endregion

        #region Possible Moves Logic

        /// <summary>
        /// Returns all valid possible moves for this micro board.
        /// </summary>
        /// <returns></returns>
        public List<PlayerMove> GetPossibleMoves()
        {
            List<PlayerMove> moves = new List<PlayerMove>();

            // If we are playable
            if(IsPlayable)
            {
                int boardOffsetX = m_thisBoardRow * 3;
                int boardOffsetY = m_thisBoardCol * 3;

                // Loop through all possible moves.
                for(int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        // If the slot is 0 list it.
                        if(m_macroBoard.Slots[x + boardOffsetX][y + boardOffsetY] == 0)
                        {
                            // If so add it.
                            moves.Add(new PlayerMove(x + boardOffsetX, y + boardOffsetY));
                        }
                    }
                }
            }

            // Return moves if any.
            return moves;
        }


        #endregion

        #region Check for Win Logic

        /// <summary>
        /// Checks the current board to see if it is won.
        /// </summary>
        public void CheckForWin()
        {
            // Get the index for this microboard
            int thisMicroBoardIndex = m_thisBoardRow + m_thisBoardCol * 3;

            if(m_macroBoard.MacroBoardStates[thisMicroBoardIndex] == 1 || m_macroBoard.MacroBoardStates[thisMicroBoardIndex] == 2)
            {
                // The board is already won
                return;
            }

            // Check if the board is wont and set the correct value
            if(HasPlayerWon(1))
            {
                m_macroBoard.MacroBoardStates[thisMicroBoardIndex] = 1;
            }
            else if (HasPlayerWon(2))
            {
                m_macroBoard.MacroBoardStates[thisMicroBoardIndex] = 2;
            }
        }

        public bool HasPlayerWon(sbyte playerId)
        {
            //0,1,2
            //3,4,5
            //6,7,8

            // Get the offsets
            int xOffset = (int)m_thisBoardRow  * 3;
            int yOffset = (int)m_thisBoardCol * 3;

            // Check for the first row, first col, and the first diag
            if (m_macroBoard.Slots[xOffset][yOffset] == playerId)
            {
                // Row
                if (m_macroBoard.Slots[xOffset+1][yOffset] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset +2][yOffset] == playerId)
                    {
                        return true;
                    }
                }
                // Diag
                if (m_macroBoard.Slots[xOffset + 1][yOffset + 1] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset + 2 ][yOffset + 2] == playerId)
                    {
                        return true;
                    }
                }
                // Col
                if (m_macroBoard.Slots[xOffset][yOffset + 1] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset][yOffset + 2] == playerId)
                    {
                        return true;
                    }
                }
            }

            // Check for mid col
            if (m_macroBoard.Slots[xOffset + 1][yOffset] == playerId)
            {
                if (m_macroBoard.Slots[xOffset + 1][yOffset + 1] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset + 1][yOffset + 2] == playerId)
                    {
                        return true;
                    }
                }
            }

            // Check for the last col
            if (m_macroBoard.Slots[xOffset + 2][yOffset] == playerId)
            {
                if (m_macroBoard.Slots[xOffset + 2][yOffset + 1] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset + 2][yOffset + 2] == playerId)
                    {
                        return true;
                    }
                }
            }

            // Check for middle row
            if (m_macroBoard.Slots[xOffset][yOffset + 1] == playerId)
            {
                if (m_macroBoard.Slots[xOffset + 1][yOffset + 1] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset + 2][yOffset + 1] == playerId)
                    {
                        return true;
                    }
                }
            }

            // Check for bottom row, and other diag
            if (m_macroBoard.Slots[xOffset][yOffset + 2] == playerId)
            {
                if (m_macroBoard.Slots[xOffset + 1][yOffset + 2] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset + 2][yOffset + 2] == playerId)
                    {
                        return true;
                    }
                }
                if (m_macroBoard.Slots[xOffset + 1][yOffset + 1] == playerId)
                {
                    if (m_macroBoard.Slots[xOffset + 2][yOffset] == playerId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
