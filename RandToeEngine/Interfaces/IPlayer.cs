using RandToeEngine.CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Interfaces
{
    public interface IPlayer
    {
        /// <summary>
        /// Called when the player should make a move.
        /// </summary>
        /// <param name="engine"></param>
        void MoveRequested(IGameEngine engine);
    }
}
