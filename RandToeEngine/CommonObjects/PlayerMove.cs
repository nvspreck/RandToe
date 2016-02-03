using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class PlayerMove
    {
        public PlayerMove(int macroX, int macroY)
        {
            MacroX= macroX;
            MacroY = macroY;
        }

        public int MacroX { get; }

        public int MacroY { get; }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            PlayerMove move = (PlayerMove)obj;
            return (move.MacroX == this.MacroX) && (this.MacroY == move.MacroY);
        }
    }
}
