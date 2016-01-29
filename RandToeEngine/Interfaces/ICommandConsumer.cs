using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Interfaces
{
    public interface ICommandConsumer
    {
        /// <summary>
        /// Fired when a new command has been made.
        /// </summary>
        /// <param name="commandString"></param>
        void OnCommandRecieved(string commandString);
    }
}
