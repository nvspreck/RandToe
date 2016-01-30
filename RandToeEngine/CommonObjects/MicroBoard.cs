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


    }
}
