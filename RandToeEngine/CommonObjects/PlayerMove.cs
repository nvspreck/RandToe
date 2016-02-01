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
    }
}
