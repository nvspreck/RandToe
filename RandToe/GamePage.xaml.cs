
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RandToe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public const string NavArg_NetworkGameId = "&NetworkGameId=";

        NetworkGameEngine m_engine;

        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if(args.Parameter.GetType() == typeof(string))
            {
                string navArg = (string)args.Parameter;
                if(navArg.StartsWith(NavArg_NetworkGameId))
                {
                    // This is a network game. Grab the id.
                    int start = navArg.IndexOf("=") + 1;

                    // Make the engine.
                    m_engine = new NetworkGameEngine(navArg.Substring(start));
                    m_engine.OnNewMoveMade += M_engine_OnNewMoveMade;
                }
            }
        }

        private void M_engine_OnNewMoveMade(PlayerMove move)
        {

        }

        private void Square_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
