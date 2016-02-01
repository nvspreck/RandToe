using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Core;

namespace RandToe
{
    class NetworkGameEngine
    {
        const string c_hubUrl = "http://randtoeweb.azurewebsites.net/";


        // Private vars
        HubConnection m_hub;
        IHubProxy m_gameProxy;
        string m_gameId;

        public NetworkGameEngine(string gameId)
        {
            m_gameId = gameId;

            Task.Run(async () =>
            {
                // Setup the hub connection.
                m_hub = new HubConnection(c_hubUrl);
                m_gameProxy = m_hub.CreateHubProxy("GameHostHub");
                m_hub.StateChanged += Hub_StateChanged;

                // Start the hub.
                await m_hub.Start();
            });
        }

        private void Hub_StateChanged(StateChange obj)
        {
            // When we are connected grab the moves again.
            if(obj.NewState == ConnectionState.Connected)
            {
                SetupGame();
            }
        }


        public async void SetupGame()
        {
            WebGame game = await m_gameProxy.Invoke<WebGame>("GetGame", m_gameId);

            if (game.Moves != null)
            {
                foreach (WebMove move in game.Moves)
                {
                    //PlayerMove playerMove = new PlayerMove(move.XCord, move.YCord, move.Player);
                    //if(OnNewMoveMade != null)
                    //{
                    //    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    //    {
                    //        OnNewMoveMade(playerMove);
                    //    });

                    //    await Task.Delay(500);
                    //}
                }
            }
        }

        #region Network Classes

        public class WebGame
        {
            public string GameId { get; set; }

            public List<WebMove> Moves { get; set; }

            public bool IsOver { get; set; }

            public string StartingPlayerId { get; set; }
        }

        public class WebMove
        {
            public int Player { get; set; }

            public int XCord { get; set; }

            public int YCord { get; set; }
        }

        #endregion
    }
}
