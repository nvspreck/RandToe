using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.CommonObjects
{
    public class PlayerMove
    {
        public PlayerMove(int playerId, int x, int y)
        {
            X = x;
            Y = y;
            PlayerId = playerId;
        }

        public int X { get; }

        public int Y { get; }

        public int PlayerId { get; }
    }
}
