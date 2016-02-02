using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class HistoricPlayerMove
    {
        public HistoricPlayerMove(int playerId, PlayerMove move)
        {
            PlayerId = playerId;
            MacroX = move.MacroX;
            MacroY = move.MacroY;
        }

        public int MacroX { get; }

        public int MacroY { get; }

        public int PlayerId { get; }
    }
}
