
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RandToe
{

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
                // Make player 1
                m_player1 = new PlayerContext();
                m_player1.PlayerNumber = 1;
                m_player1.Callback = UpdateGameState;
                m_player1.Engine = new RandToeEngineCore(null, m_player1);

                // make player 2
                m_player2 = new PlayerContext();
                m_player2.PlayerNumber = 2;
                m_player2.Callback = UpdateGameState;
                m_player2.Engine = new RandToeEngineCore(null, m_player2);

                m_player1.Engine.OnCommandRecieved("update game field 0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
                m_player1.Engine.OnCommandRecieved("update game macroboard -1,0,0,0,0,0,0,0,0");
                m_player1.Engine.OnCommandRecieved("action move 1000");
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
