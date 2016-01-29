using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class MicroBoard
    {
        MacroBoard m_macroBoard;

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
        { }

        #endregion


    }
}
