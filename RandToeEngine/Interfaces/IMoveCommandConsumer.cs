using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Interfaces
{
    public interface IMoveCommandConsumer
    {
        /// <summary>
        /// Called when someone wants to send the move command.
        /// </summary>
        /// <param name="moveCommand"></param>
        void OnMakeMoveCommand(string moveCommand);

    }
}
