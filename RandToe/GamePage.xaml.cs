
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
            PlayerMove pm = new PlayerMove(x, y);
            List<PlayerMove> avaliableMoves = m_board.GetAllPossibleMoves();

            return avaliableMoves.Contains(pm);
        }

        private void MakeMove(int x, int y)
        {
            m_board.Slots[x][y] = m_currentTurn.PlayerId;
        }

        private bool IsGameOver()
        {
            return m_board.HasPlayerWon(m_move);
        }

        private void ToggleCurrentPlayer()
        {
            if (m_currentTurn == m_playerOne)
            {
                m_currentTurn = m_playerTwo;
            }
            else
            {
                m_currentTurn = m_playerOne;
            }
        }

        private void UpdateMoveRound()
        {
            m_move += 1;
            if (m_move == 3)
            {
                m_round += 1;
                m_move = 1;
            }
        }

        private void SendMoveToCurrentPlayer()
        {
            string round1 = "update game round " + m_round;
            string updatemove1 = "update game move " + m_move;
            string gamefield1 = "update game field"; // plus the board state
            string macroboard1 = "update game macroboard"; // plus the macro board state
            string move = "action move 10000";

            m_currentTurn.OnCommandRecieved(round1);
            m_currentTurn.OnCommandRecieved(updatemove1);
            m_currentTurn.OnCommandRecieved(gamefield1);
            m_currentTurn.OnCommandRecieved(macroboard1);
            m_currentTurn.OnCommandRecieved(move);
        }

        private void EndGame()
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
                return;
            }

            int x = Int32.Parse(commandParts[1]);
            int y = Int32.Parse(commandParts[2]);

            if (IsLegalMove(x, y))
            {
                MakeMove(x, y);
                if (IsGameOver())
                {
                    EndGame();
                }
                else
                {
                    ToggleCurrentPlayer();
                    UpdateMoveRound();
                    SendMoveToCurrentPlayer();
                }
            }
        }

        public void Run(PlayerBase playerOne, PlayerBase playerTwo)
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

            m_playerOne.OnCommandRecieved(timeBank);
            m_playerOne.OnCommandRecieved(timePerMove);
            m_playerOne.OnCommandRecieved(names);
            m_playerOne.OnCommandRecieved(yourname1);
            m_playerOne.OnCommandRecieved(yourid1);

            m_playerTwo.OnCommandRecieved(timeBank);
            m_playerTwo.OnCommandRecieved(timePerMove);
            m_playerTwo.OnCommandRecieved(names);
            m_playerTwo.OnCommandRecieved(yourname2);
            m_playerTwo.OnCommandRecieved(yourid2);

            m_currentTurn = m_playerOne;
            m_round = 1;
            m_move = 1;

            sbyte[] intArray = new sbyte[81];

            for (int i = 0; i < 81; i++)
            {
                intArray[i] = 0;
            }

            m_board = MacroBoard.CreateNewBoard(1,intArray);
            sbyte[] macroBoard = { -1, -1, -1, -1, -1, -1, -1, -1, -1};
            MacroBoard.AddMacroboardData(m_board, macroBoard);


            SendMoveToCurrentPlayer();
        }

        private PlayerBase m_playerOne;
        private PlayerBase m_playerTwo;
        private PlayerBase m_currentTurn;
        private sbyte m_round;
        private sbyte m_move;
        private MacroBoard m_board;
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
            //Set up the game backend
            GameRunner gameRunner = new GameRunner();


            // Setup the human player
            PlayerBase humanPlayer = new PlayerBase(gameRunner, this);
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
                        humanOrBotPlayer = new PlayerBase(gameRunner, this);
                    }
                    else
                    {
                        humanOrBotPlayer = new PlayerBase(gameRunner, BotManager.GetBot(botName));
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
                gameRunner.Run(m_player1, m_player2);
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
                int otherPlayerId = playerBase.PlayerId == 1 ? 2 : 1;
                HistoricPlayerMove lastMove = playerBase.HistoricMoves.Count > 0 ? playerBase.HistoricMoves.Last<HistoricPlayerMove>() : null;

                // Update the UI.
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        int microBoardNum = ((int)Math.Floor(x / 3.0) + (int)Math.Floor(y / 3.0) * 3);

                        // Fix the offset
                        int microY = y - (int)Math.Floor(microBoardNum / 3.0) * 3;
                        int microX = x - (int)microBoardNum % 3 * 3;

                        // Update the text
                        TextBlock textBlock = (TextBlock)FindName($"ui_text_{microBoardNum}_{microX}_{microY}");
                        int value = playerBase.CurrentBoard.Slots[x][y];
                        textBlock.Text = value == 0 ? "" : (value == 1 ? "1" : "2");

                        // Update the last played cell and active box color.
                        Grid celGrid = (Grid)FindName($"ui_grid_{microBoardNum}_{microX}_{microY}");
                        MicroBoard microBoard = playerBase.CurrentBoard.GetMicroBoardForMacroCords(x, y);

                        if (lastMove != null && lastMove.MacroX == x && lastMove.MacroY == y)
                        {
                            // The last move made.
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0));
                        }
                        else if (microBoard.IsPlayable)
                        {
                            // Is playable
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(75, 255, 255, 0));
                        }
                        else if (microBoard.BoardState == playerBase.PlayerId)
                        {
                            // We won
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                        }
                        else if (microBoard.BoardState == otherPlayerId)
                        {
                            // They won
                            celGrid.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                        }
                        else
                        {
                            // nothing.
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
