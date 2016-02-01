
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
    public sealed partial class GamePage : Page
    {
        public const string NavArg_NetworkGameId = "&NetworkGameId=";

        NetworkGameEngine m_engine;

        bool m_isPlayerOneTurn = true;
        bool m_isWaitingOnInput = false;
        PlayerContext m_player1;
        PlayerContext m_player2;

        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if(args.Parameter != null && args.Parameter.GetType() == typeof(string))
            {
                string navArg = (string)args.Parameter;
                if(navArg.StartsWith(NavArg_NetworkGameId))
                {
                    // This is a network game. Grab the id.
                    int start = navArg.IndexOf("=") + 1;

                    // Make the engine.
                    m_engine = new NetworkGameEngine(navArg.Substring(start));
                }
            }
            else
            {
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
        }

        private async void UpdateGameState(PlayerContext context)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                // Set the player text
                ui_readyPlayerText.Text = $"Player {context.PlayerNumber}'s Turn";

                // Update the UI.
                for(int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        int microBoard = ((int)Math.Floor(x / 3.0) + (int)Math.Floor(y / 3.0) * 3);

                        // Fix the offset
                        int microY = y - (int)Math.Floor(microBoard / 3.0) * 3;
                        int microX = x - (int)microBoard % 3 * 3;

                        TextBlock textBlock = (TextBlock)FindName($"ui_text_{microBoard}_{microX}_{microY}");
                        int value = context.Engine.CurrentBoard.Slots[x][y];
                        textBlock.Text = value == 0 ? "" : (value == 1 ? "1" : "2");


                    }
                }

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
            if(!m_isWaitingOnInput)
            {
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

            Task.Run(() =>
            {
                // Send to the engine.
                if (m_isPlayerOneTurn)
                {
                    m_player1.Engine.MakeMove(x, y);
                }
                else
                {
                    m_player2.Engine.MakeMove(x, y);
                }
            });
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ui_mainGrid.MaxHeight = e.NewSize.Width;
        }
    }
}
