using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RandToe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NetworkGameSetup : Page
    {
        const string c_hubUrl = "http://randtoeweb.azurewebsites.net/";


        HubConnection m_hub;
        IHubProxy m_gameProxy;


        public NetworkGameSetup()
        {
            this.InitializeComponent();

            // Kick to the background.
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


        private async void Hub_StateChanged(StateChange obj)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                switch (obj.NewState)
                {
                    case ConnectionState.Connected:
                        ui_stateText.Text = "Getting Games...";
                        DoHubConnected();
                        break;
                    case ConnectionState.Disconnected:
                        ui_stateText.Text = "Disconnected";
                        break;
                    case ConnectionState.Reconnecting:
                        ui_stateText.Text = "Reconnected...";
                        break;
                    case ConnectionState.Connecting:
                        ui_stateText.Text = "Connecting...";
                        break;
                }
            });
        }

        private async void DoHubConnected()
        {
            // Get the game Ids
            List<string> gameIdList = await m_gameProxy.Invoke<List<string>>("GetAllGameIds");

            // Add if we have none.
            if(gameIdList.Count == 0)
            {
                gameIdList.Add("None");
            }

            // Set the list.
            ui_gameList.ItemsSource = gameIdList;

            // Give an example name.
            ui_newGameName.Text = gameIdList.Count.ToString();

            // Set the text
            ui_stateText.Text = "Done";
        }

        private async void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if(m_hub != null && m_hub.State == ConnectionState.Connected)
            {
                // Get the user id                
                string userId = null;
                IReadOnlyList<User> users = await Windows.System.User.FindAllAsync();
                foreach(User user in users)
                {
                    userId = user.NonRoamableId;
                }

                // Get the game id
                string gameId = ui_newGameName.Text;

                // Make the request.
                object[] args = { gameId, userId };
                bool success = await m_gameProxy.Invoke<bool>("CreateGame", args);

                if(success)
                {
                    GoToGame(gameId);
                }
                else
                {
                    MessageDialog message = new MessageDialog("The game id must be unique!", "Failed");
                    await message.ShowAsync();
                }
            }
        }

        private void GameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ui_gameList.SelectedIndex == -1)
            {
                return;
            }

            GoToGame((string)ui_gameList.SelectedItem);
        }

        private void GoToGame(string gameId)
        {
            this.Frame.Navigate(typeof(GamePage), GamePage.NavArg_NetworkGameId + gameId);
        }
    }
}
