using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandToeEngine.Interfaces;
using RandToeEngine;
using System.IO;
using System.Diagnostics;

namespace RandToeConsoleRunner
{
    class Program : IMoveCommandConsumer
    {
        /// <summary>
        /// The text write used for writing output.
        /// </summary>
        TextWriter m_consoleOut;

        /// <summary>
        /// The text write used for writing error.
        /// </summary>
        TextWriter m_consoleError;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        /// <summary>
        /// Main logic for the program.
        /// </summary>
        public void Run()
        {
            // Setup
            m_consoleError = Console.Error;
            m_consoleOut = Console.Out;
            TextReader consoleReader = Console.In;

            // Sub to the logger
            RandToeEngineCore.Logger.OnLogMessage += Logger_OnLogMessage;

            RandToeEngineCore.Logger.Log(this, "Creating Engine");

            // Make the engine.
            RandToeEngineCore engine = new RandToeEngineCore(this, null);

            RandToeEngineCore.Logger.Log(this, "Engine Created; Entering main loop");

            // The main loop
            while(true)
            {
                try
                {
                    RandToeEngineCore.Logger.Log(this, "Waiting for command");

                    // Read a new line.
                    string newCommand = consoleReader.ReadLine();

                    RandToeEngineCore.Logger.Log(this, $"Command Received: {newCommand}");

                    // Send the command to the engine
                    engine.OnCommandRecieved(newCommand);
                }
                catch(Exception ex)
                {
                    RandToeEngineCore.Logger.Log(this, "Exception in main logic loop!", ex);
                }
            }
        }


        #region IMoveCommandConsumer

        /// <summary>
        /// Fired when the engine wants to send a command.
        /// </summary>
        /// <param name="moveCommand"></param>
        public void OnMakeMoveCommand(string moveCommand)
        {
            if(m_consoleOut != null)
            {
                m_consoleOut.WriteLine(moveCommand);
            }
            else
            {
                RandToeEngineCore.Logger.Log(this, $"OnMakeMoveCommand was called but we didn't have an output!", LogLevels.Error);
            }
        }

        #endregion

        private void Logger_OnLogMessage(LogLevels level, string formattedMessage)
        {
            if (m_consoleError != null)
            {
                // Fire off the message async.
                m_consoleError.WriteLineAsync(formattedMessage);
            }
        }
    }
}
