using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToe
{
    /// <summary>
    /// Possible logging levels.
    /// </summary>
    public enum LogLevels
    {
        Info = 0,
        Warn = 1,
        Error = 2
    };

    public delegate void OnLogMessageDelegate(LogLevels level, string formattedMessage);

    public class Logger
    {
        /// <summary>
        /// An event for anyone to sub to log messages.
        /// </summary>
        public event OnLogMessageDelegate OnLogMessage;

        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Log(object source, string message, Exception ex)
        {
            Log(source, "EXCP! ("+message+") Exc Mes: ("+ex.Message+")", LogLevels.Error);
        }

        /// <summary>
        /// Logs a message at information level.
        /// </summary>
        /// <param name="message"></param>
        public void Log(object source, string message, LogLevels level = LogLevels.Info)
        {
            // Get the level
            string levelString;
            switch(level)
            {
                case LogLevels.Info:
                    levelString = "Info";
                    break;
                case LogLevels.Warn:
                    levelString = "Warn";
                    break;
                default:
                case LogLevels.Error:
                    levelString = "Error";
                    break;
            }

            // Make the formatted string.
            // todo enable
            string formattedMessage = /*DateTime.Now.Hour.ToString("D2")+":"+DateTime.Now.Minute.ToString("D2")+":"+DateTime.Now.Second.ToString("D2")+":"+DateTime.Now.Millisecond.ToString("D3")+" ["+levelString+"] ("+source.GetType().Name+"): "+*/ message;

            // Send the log.
            if(OnLogMessage != null)
            {
                OnLogMessage(level, formattedMessage);
            }

#if DEBUG
            // If we are in debug write to the output window also.
            Debug.WriteLine(formattedMessage);
#endif
        }
    }
}
