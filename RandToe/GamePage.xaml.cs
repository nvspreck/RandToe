
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RandToe
{
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
                m_player1.OnCommandRecieved("update game field 0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
                m_player1.OnCommandRecieved("update game macroboard -1,0,0,0,0,0,0,0,0");
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
