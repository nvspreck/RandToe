using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToe
{
    public class PlayerMove
    {
        public PlayerMove(int macroX, int macroY)
        {
            m_macroX = macroX;
            m_macroY = macroY;
        }

        public int MacroX { get { return m_macroX; } }
        private int m_macroX = 0;

        public int MacroY { get { return m_macroY; } }
        private int m_macroY = 0;

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            PlayerMove move = (PlayerMove)obj;
            return (move.MacroX == this.MacroX) && (this.MacroY == move.MacroY);
        }

        public override int GetHashCode()
        {
            return this.MacroX + (this.MacroY * 255);
        }
    }
}
