using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToe
{
    public class HistoricPlayerMove
    {
        public HistoricPlayerMove(int playerId, PlayerMove move)
        {
            m_playerId = playerId;
            m_macroX = move.MacroX;
            m_macroY = move.MacroY;
        }

        public int MacroX { get { return m_macroX; } }
        private int m_macroX = 0;

        public int MacroY { get { return m_macroY; } }
        private int m_macroY = 0;

        public int PlayerId { get { return m_playerId; } }
        private int m_playerId = 0;
    }
}
