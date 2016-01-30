using RandToeEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandToeEngine.CommonObjects;

namespace RandToeEngine
{
    /// <summary>
    /// The base logic for any player
    /// </summary>
    public class PlayerBase: ICommandConsumer, IPlayerBase
    {
        /// <summary>
        /// This is our static logger. This can be used by anyone to log events.
        /// </summary>
        public static Logger Logger = new Logger();

        /// <summary>
        /// The interface we will use to send move commands out.
        /// </summary>
        readonly IMoveCommandConsumer m_moveConsumer;

        /// <summary>
        /// Holds a reference to the player that is playing.
        /// </summary>
        readonly IPlayer m_player;


        public PlayerBase(IMoveCommandConsumer moveConsumer, IPlayer player)
        {
            m_moveConsumer = moveConsumer;
            m_player = player;
        }

        #region ICommandConsumer

        /// <summary>
        /// Fired when we have a new command to deal with.
        /// </summary>
        /// <param name="commandString"></param>
        public void OnCommandRecieved(string commandString)
        {
            string commandLower = commandString.ToLower();

            // break the command into parts.
            string[] commandParts = commandLower.Split(' ');

            if(commandParts.Length < 2)
            {
                Logger.Log(this, "The command is invalid! It has less than 2 parts.", LogLevels.Error);
                return;
            }

            if(commandParts[0].Equals("settings"))
            {
                // Parse settings
                ParseSettings(commandParts, commandString);
            }
            else if (commandParts[0].Equals("update"))
            {
                // Parse updates
                ParseUpdate(commandParts);
            }
            else if (commandParts[0].Equals("action"))
            {
                // Parse action
                ParseAction(commandParts);
            }
            else
            {
                Logger.Log(this, $"An unkown command base has been sent! ({commandParts[0]})", LogLevels.Error);
            }
        }

        /// <summary>
        /// Parses a settings command
        /// </summary>
        /// <param name="commandParts"></param>
        /// <param name="orgionalCommand"></param>
        private void ParseSettings(string[] commandParts, string orgionalCommand)
        {
            if (commandParts.Length < 3)
            {
                Logger.Log(this, $"Settings command is invalid, it has less than 3 parts!", LogLevels.Error);
                return;
            }

            switch (commandParts[1])
            {
                case "timebank":
                    int newTimebank;
                    if(int.TryParse(commandParts[2], out newTimebank))
                    {
                        Timebank = newTimebank;
                        Logger.Log(this, $"Timebank updated ({Timebank})");
                    }
                    else
                    {
                        Logger.Log(this, $"Timebank command invalid ({commandParts[2]})", LogLevels.Error);
                    }
                    break;
                case "time_per_move":
                    int newTimePerMove;
                    if (int.TryParse(commandParts[2], out newTimePerMove))
                    {
                        TimeAddedPerMove = newTimePerMove;
                        Logger.Log(this, $"TimeAddedPerMove updated ({TimeAddedPerMove})");

                    }
                    else
                    {
                        Logger.Log(this, $"time_per_move command invalid ({commandParts[2]})", LogLevels.Error);
                    }
                    break;
                case "player_names":
                    // Get the original arg so the case is correct
                    string[] orgCmdParts = orgionalCommand.Split(' ');
                    Players = orgCmdParts[2].Split(',').ToList<string>();
                    Logger.Log(this, $"Players updated ({Players.ToString()})");
                    break;
                case "your_bot":
                    // Get the original arg so the case is correct
                    string[] orgCmdPartsBot = orgionalCommand.Split(' ');
                    PlayerName = orgCmdPartsBot[2];
                    Logger.Log(this, $"PlayerName updated ({PlayerName})");
                    break;
                case "your_botid":
                    int botId;
                    if (int.TryParse(commandParts[2], out botId))
                    {
                        PlayerId = botId;
                        Logger.Log(this, $"PlayerId updated ({PlayerId})");
                    }
                    else
                    {
                        Logger.Log(this, $"your_botid command invalid ({commandParts[2]})", LogLevels.Error);
                    }
                    break;
                default:
                    Logger.Log(this, $"An unkown settings command has been sent! ({commandParts[1]})", LogLevels.Error);
                    break;
            }
        }

        /// <summary>
        /// Parses an update command
        /// </summary>
        /// <param name="commandParts"></param>
        private void ParseUpdate(string[] commandParts)
        {
            // Validate
            if(!commandParts[1].Equals("game"))
            {
                Logger.Log(this, $"An unkown update command has been sent! ({commandParts[1]})", LogLevels.Error);
                return;
            }
            if (commandParts.Length < 3)
            {
                Logger.Log(this, $"Update command is invalid, it has less than 3 parts!", LogLevels.Error);
                return;
            }

            switch (commandParts[2])
            {
                case "round":
                    int newRound;
                    if (int.TryParse(commandParts[2], out newRound))
                    {
                        CurrentRound = newRound;
                        Logger.Log(this, $"CurrentRound updated ({CurrentRound})");
                    }
                    else
                    {
                        Logger.Log(this, $"round command invalid ({commandParts[2]})", LogLevels.Error);
                    }
                    break;
                case "move":
                    int newMove;
                    if (int.TryParse(commandParts[2], out newMove))
                    {
                        CurrentMove = newMove;
                        Logger.Log(this, $"CurrentMove updated ({CurrentMove})");
                    }
                    else
                    {
                        Logger.Log(this, $"move command invalid ({commandParts[2]})", LogLevels.Error);
                    }
                    break;
                case "field":
                    // Parse the value
                    sbyte[] fieldArray = ParseBoardByteArray(commandParts[3], 81);
                    if(fieldArray != null)
                    {
                        SetNewBoardValue(fieldArray, null);
                    }
                    break;
                case "macroboard":
                    // Parse the value
                    sbyte[] macroArray = ParseBoardByteArray(commandParts[3], 9);
                    if (macroArray != null)
                    {
                        SetNewBoardValue(null, macroArray);
                    }
                    break;
                default:
                    Logger.Log(this, $"An unkown update command has been sent! ({commandParts[2]})", LogLevels.Error);
                    break;
            }
        }

        /// <summary>
        /// Parses an byte array.
        /// </summary>
        /// <param name="intArrayString"></param>
        /// <param name="expectedCount"></param>
        /// <returns></returns>
        private sbyte[] ParseBoardByteArray(string intArrayString, int expectedCount)
        {
            // Parse the value
            sbyte[] intArray = new sbyte[expectedCount];
            string[] fieldStringArray = intArrayString.Split(',');
            int count = 0;
            foreach (string field in fieldStringArray)
            {
                if (!sbyte.TryParse(fieldStringArray[count], out intArray[count]))
                {
                    Logger.Log(this, $"Failed to parse int array pos:({count}) string:({intArrayString})", LogLevels.Error);
                }
                count++;
            }

            // Validate
            if (count != expectedCount)
            {
                Logger.Log(this, $"Int array didn't contain {expectedCount} elements!", LogLevels.Error);
                return null;
            }

            return intArray;
        }

        /// <summary>
        /// Sets a new value to the game board.
        /// </summary>
        /// <param name="fieldArray"></param>
        /// <param name="macroBoard"></param>
        private void SetNewBoardValue(sbyte[] fieldArray, sbyte[] macroBoard)
        {
            // If we have a field set
            if(fieldArray != null)
            {
                // Check that the board is old.
                if(CurrentBoard != null && CurrentBoard.Round == CurrentRound)
                {
                    Logger.Log(this, $"We are creating a new board with a new field value but the round is the same! Round:({CurrentRound})", LogLevels.Warn);
                }

                // Make a new board
                CurrentBoard = MacroBoard.CreateNewBoard(CurrentRound, fieldArray);
            }

            // If we have a macro board set it.
            if(macroBoard != null)
            {
                // Check that the board is old.
                if (CurrentBoard != null && CurrentBoard.Round != CurrentRound)
                {
                    Logger.Log(this, $"We are setting the macroboard on a board that is not the current round number! Current Round:({CurrentRound}); Board Round ({CurrentBoard.Round})", LogLevels.Error);
                }
                if(CurrentBoard != null && CurrentBoard.HasAllData)
                {
                    Logger.Log(this, $"Macroboard data set on a board that already has all data", LogLevels.Error);
                    return;
                }

                // Set the values
                MacroBoard.AddMacroboardData(CurrentBoard, macroBoard);
            }
        }

        /// <summary>
        /// Parses an action.
        /// </summary>
        /// <param name="commandParts"></param>
        private void ParseAction(string[] commandParts)
        {
            if(!commandParts[1].Equals("move"))
            {
                Logger.Log(this, $"An unkown action command has been sent! ({commandParts[1]})", LogLevels.Error);
                return;
            }

            // Validate
            if(CurrentBoard == null)
            {
                Logger.Log(this, $"We were asked to make a move but we don't have a board!", LogLevels.Error);
                return;
            }
            if (CurrentBoard.Round != CurrentRound)
            {
                Logger.Log(this, $"We were asked to make a move but the board is from an older round!", LogLevels.Error);
            }
            if (!CurrentBoard.HasAllData)
            {
                Logger.Log(this, $"We were asked to make a move our board doesn't have all of the data!", LogLevels.Error);
            }

            // Ask the player to make a move
            try
            {
                m_player.MoveRequested(this);
            }
            catch(Exception ex)
            {
                Logger.Log(this, $"Exception in IPlayer Move Requested!", ex);
            }
        }

        #endregion

        #region IGameEngine

        /// <summary>
        /// The current game board.
        /// </summary>
        public MacroBoard CurrentBoard { get; private set; }

        /// <summary>
        /// The number of the current move
        /// </summary>
        public int CurrentMove { get; private set; }

        /// <summary>
        /// The current round number
        /// </summary>
        public int CurrentRound { get; private set; }

        /// <summary>
        /// The player Id
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// The player name
        /// </summary>
        public string PlayerName { get; private set; }

        /// <summary>
        /// The list of current players
        /// </summary>
        public List<string> Players { get; private set; }

        /// <summary>
        /// A list of past moves.
        /// </summary>
        public List<PlayerMove> PreviousMoves { get; private set; }

        /// <summary>
        /// The time we get per move
        /// </summary>
        public int TimeAddedPerMove { get; private set; }

        /// <summary>
        /// The total amount of time we have to think.
        /// </summary>
        public int Timebank { get; private set; }

        /// <summary>
        /// Called when we want to make a move.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool MakeMove(PlayerMove move)
        {
            // Validate the move, make sure there isn't already a play here.
            if(CurrentBoard.Slots[move.MacroX][move.MacroY] != 0)
            {
                Logger.Log(this, $"Make move has been called with an illegal move! The space is not empty", LogLevels.Error);
                return false;
            }

            // Validate they are playing in a box that can be played in
            if(!CurrentBoard.GetMicroBoardForMacroCords(move.MacroX, move.MacroY).IsPlayable)
            {
                Logger.Log(this, $"Make move has been called with an illegal move! The board is not playable!", LogLevels.Error);
                return false;
            }

            // Make the move
            m_moveConsumer.OnMakeMoveCommand($"place_move {move.MacroX} {move.MacroY}");
            return true;
        }

        #endregion
    }
}
