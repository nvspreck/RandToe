
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RandToeEngine;
using RandToeEngine.Interfaces;
using System.Threading.Tasks;
using Windows.UI.Core;
using RandToeEngine.Bots;
using Windows.UI.Popups;
using Windows.UI;
using RandToeEngine.CommonObjects;
using Windows.UI.Xaml.Media.Animation;


using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RandToe
{

    class GameRunner : IMoveCommandConsumer
    {

        private bool IsLegalMove(int x, int y)
        {


        }

        private void MakeMove(int x, int y)
        {

        }

        private bool IsGameOver()
        {

        }

        /// <summary>
        /// Called when someone wants to send the move command.
        /// </summary>
        /// <param name="moveCommand"></param>
        public void OnMakeMoveCommand(string moveCommand)
        {

            string commandLower = moveCommand.ToLower();

            // break the command into parts.
            string[] commandParts = commandLower.Split(' ');

            if (commandParts.Length < 3)
            {
                //Logger.Log(this, "The command is invalid! It has less than 3 parts.", LogLevels.Error);
                return;
            }

            int x = Int32.Parse(commandParts[1]);
            int y = Int32.Parse(commandParts[2]);

            if (IsLegalMove(x, y))
            {
                MakeMove(x, y);
                if (IsGameOver())
                {

                }
                else
                {
                    //toggle current player
                    //update move / round
                    //send action move to new current player
                }
            }

      
        }

        public void Run(PlayerContext playerOne, PlayerContext playerTwo)
        {
            m_playerOne = playerOne;
            m_playerTwo = playerTwo;


            // tell them the setting and shit everything the server says it will to both players
            string timeBank = "settings timebank 10000";
            string timePerMove = "settings time_per_move 500";
            string names = "settings player_names player1,player2";
            string yourname1 = "settings your_bot player1";
            string yourname2 = "settings your_bot player2";
            string yourid1 = "settings your_botid 1";
            string yourid2 = "settings your_botid 2";

            m_playerOne.Engine.OnCommandRecieved(timeBank);
            m_playerOne.Engine.OnCommandRecieved(timePerMove);
            m_playerOne.Engine.OnCommandRecieved(names);
            m_playerOne.Engine.OnCommandRecieved(yourname1);
            m_playerOne.Engine.OnCommandRecieved(yourid1);

            m_playerTwo.Engine.OnCommandRecieved(timeBank);
            m_playerTwo.Engine.OnCommandRecieved(timePerMove);
            m_playerTwo.Engine.OnCommandRecieved(names);
            m_playerTwo.Engine.OnCommandRecieved(yourname2);
            m_playerTwo.Engine.OnCommandRecieved(yourid2);


            string round1 = "update game round 1";
            string move1 = "update game move 1";
            string gamefield1 = "update game field 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
            string macroboard1 = "update game macroboard - 1,-1,-1,-1,-1,-1,-1,-1,-1";
            string move1 = "action move 10000";

            m_playerOne.Engine.OnCommandRecieved(round1);
            m_playerOne.Engine.OnCommandRecieved(move1);
            m_playerOne.Engine.OnCommandRecieved(gamefield1);
            m_playerOne.Engine.OnCommandRecieved(macroboard1);
            m_playerOne.Engine.OnCommandRecieved(move1);
            
        }

        private PlayerContext m_playerOne;
        private PlayerContext m_playerTwo;
        private PlayerContext m_currentTurn;
    }

    class PlayerContext : IPlayer
    {
        public int PlayerNumber;
        public RandToeEngineCore Engine;
        public Action<PlayerContext> Callback;

        public void MoveRequested(IGameEngine engine)
        {
            Callback(this);
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page, IPlayer
    {
        // Strings used for nav args.
        public const string NavArg_NetworkGameId = "&NetworkGameId=";
        public const string NavArg_LocalBotName = "&LocalBotName=";
        public const string NavArg_LocalPlayerName = "Local Player";

        /// <summary>
        /// Indicates if it is player 1's turn
        /// </summary>
        bool m_isPlayerOneTurn = true;

        /// <summary>
        /// Indicates if we are waiting on user input.
        /// </summary>
        bool m_isWaitingOnInput = false;

        /// <summary>
        /// The first player
        /// </summary>
        PlayerBase m_player1;

        /// <summary>
        /// The second player
        /// </summary>
        PlayerBase m_player2;

        public GamePage()
        {
            this.InitializeComponent();

            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();
            var info = new ContinuumNavigationTransitionInfo();
            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            // Setup for logger callbacks.
            PlayerBase.Logger.OnLogMessage += Logger_OnLogMessage;
        }

        /// <summary>
        /// The page is being navigated to.
        /// </summary>
        /// <param name="args"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs args)
        {
            // Setup the human player
            PlayerBase humanPlayer = new PlayerBase(null, this);
            PlayerBase humanOrBotPlayer = null;

            // Figure out if player 2 is a bot or a local player.
            bool isOtherPlayerBot = false;
            if (args.Parameter != null && args.Parameter.GetType() == typeof(string))
            {
                string navArg = (string)args.Parameter;
                if(navArg.StartsWith(NavArg_LocalBotName))
                {
                    // This is a network game. Grab the id.
                    int start = navArg.IndexOf("=") + 1;
                    string botName = navArg.Substring(start);

                    if(botName.Equals(NavArg_LocalPlayerName))
                    {
                        // If we are playing a local player set it up as a player.
                        humanOrBotPlayer = new PlayerBase(null, this);
                    }
                    else
                    {
                        humanOrBotPlayer = new PlayerBase(null, BotManager.GetBot(botName));
                        isOtherPlayerBot = true;
                    }
                }
            }


            /*
            
<<<<<<< HEAD
                GameRunner gameRunner = new GameRunner();

                // Make player 1

                m_player1 = new PlayerContext();
                m_player1.PlayerNumber = 1;
                m_player1.Callback = UpdateGameState;
                m_player1.Engine = new RandToeEngineCore(gameRunner, m_player1);
                // make player 2

                m_player2 = new PlayerContext();
                m_player2.PlayerNumber = 2;
                m_player2.Callback = UpdateGameState;
                m_player2.Engine = new RandToeEngineCore(gameRunner, m_player2);

                RandToeEngineCore.Logger.Log(this, "Created Players");

                gameRunner.Run(m_player1, m_player2);
                
            }
=======
*/
            // Ask the user how should go first
            bool playerOneFirst = true;
            MessageDialog message = new MessageDialog("Who should play first?", "Ready Player 1?");
            message.Commands.Add(new UICommand("Human", (command) =>
            {
                playerOneFirst = true;
            }));
            message.Commands.Add(new UICommand(isOtherPlayerBot ? "Bot" : "Human 2", (command) =>
            {
                playerOneFirst = false;
            }));
            await message.ShowAsync();

            // Set the players
            m_player1 = playerOneFirst ? humanPlayer : humanOrBotPlayer;
            m_player2 = playerOneFirst ? humanOrBotPlayer : humanPlayer;

            await Task.Run(() =>
            {
                // Push the game to update.
                m_player1.OnCommandRecieved("update game field 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
                m_player1.OnCommandRecieved("update game macroboard 0,0,0,0,-1,0,0,0,0");
                m_player1.OnCommandRecieved("action move 1000");
            });
        }

        /// <summary>
        /// Called when we need the human to make a play.
        /// </summary>
        /// <param name="playerBase"></param>
        public async void MoveRequested(IPlayerBase playerBase)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                // Set the player text
                ui_readyPlayerText.Text = $"Player {playerBase.PlayerId}'s Turn";

                m_isPlayerOneTurn = playerBase.PlayerId == 1;

                // Update the UI.
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        int microBoard = ((int)Math.Floor(x / 3.0) + (int)Math.Floor(y / 3.0) * 3);

                        // Fix the offset
                        int microY = y - (int)Math.Floor(microBoard / 3.0) * 3;
                        int microX = x - (int)microBoard % 3 * 3;

                        // Update the text
                        TextBlock textBlock = (TextBlock)FindName($"ui_text_{microBoard}_{microX}_{microY}");
                        int value = playerBase.CurrentBoard.Slots[x][y];
                        textBlock.Text = value == 0 ? "" : (value == 1 ? "1" : "2");

                        // Update the last played cell and active box color.
                        Grid celGrid = (Grid)FindName($"ui_grid_{microBoard}_{microX}_{microY}");
                        if (value == 1)
                        {
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(75, 255, 255, 0));
                        }
                        else if (playerBase.CurrentBoard.GetMicroBoardForMacroCords(x, y).IsPlayable)
                        {
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                        }
                        else
                        {
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        }
                    }
                }

                // Set that we can take input.
                m_isWaitingOnInput = true;
            });
        }


        /// <summary>
        /// Fired when a square is tapped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Square_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Return if we don't want input.
            if(!m_isWaitingOnInput)
            {
                PlayerBase.Logger.Log(this, "User move ingored, waiting on the AI");
                return;
            }
            m_isWaitingOnInput = false;

            // Get the name
            Grid grid = (Grid)sender;
            string[] split = grid.Name.Split('_');

            // Get the x y
            int microBoard = int.Parse(split[2]);
            int y = int.Parse(split[3]);
            int x = int.Parse(split[4]);

            // Fix the offset
            y += (int)Math.Floor(microBoard / 3.0) * 3;
            x += (int)microBoard % 3 * 3;

            Task.Run(async () =>
            {
                PlayerBase engine = m_isPlayerOneTurn ? m_player1 : m_player2;

                if(!engine.MakeMove(new PlayerMove(x, y)))
                {
                    m_isWaitingOnInput = true;
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        MessageDialog message = new MessageDialog("That move is invalid, try again.", "Invalid Move");
                        await message.ShowAsync();
                    });
                }
            });
        }


        /// <summary>
        /// Fired when the grid size is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {


        }

        /// <summary>
        /// Fired when there is a new logger message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="formattedMessage"></param>
        private async void Logger_OnLogMessage(LogLevels level, string formattedMessage)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                ui_gameLog.Text += formattedMessage + Environment.NewLine;
            });
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int size = (int)Math.Min(e.NewSize.Width, 500);
            ui_mainGrid.Height = size;
            ui_mainGrid.Width = size;
        }
    }
}
