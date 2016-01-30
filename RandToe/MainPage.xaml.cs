using RandToeEngine.Bots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RandToe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();
            var info = new ContinuumNavigationTransitionInfo();
            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            // Make the list of bots, set them to the UI.
            List<string> botNames = BotManager.GetBotNames();
            botNames.Insert(0, GamePage.NavArg_LocalPlayerName);
            ui_botSelector.ItemsSource = botNames;
            ui_botSelector.SelectedIndex = 0;
        }


        /// <summary>
        /// Fired when we should go to the game page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayLocal_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GamePage), GamePage.NavArg_LocalBotName + (string)ui_botSelector.SelectedItem);
        }
    }
}
