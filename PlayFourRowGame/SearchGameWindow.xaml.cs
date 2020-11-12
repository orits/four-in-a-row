using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PlayFourRowGame.FourRowServiceReference;

namespace PlayFourRowGame
{
    /// <summary>
    /// Interaction logic for SearchGameWindow.xaml
    /// this window is for search game options (live games or history games).
    /// </summary>
    public partial class SearchGameWindow : Window
    {
        private bool _isHistoryGame = false; // is window at history mode.
        public SearchGameWindow(bool isHistoryGame)
        {
            InitializeComponent();
            _isHistoryGame = isHistoryGame;
        }

        public string Username { get; set; } // user name
        public FourRowServiceClient Client { get; set; } // client ref.

        /// <summary>
        /// this function is start when window is loaded.
        /// </summary>
        private void SearchGameWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WaitingRoomWindow.CheckPingToServer(Client))
                {
                    if (_isHistoryGame) // is history mode, ask the server for history records.
                        LbGames.ItemsSource = Client.ShowAllHistoryGames();
                    else  // is live mode, ask the server for live records.
                        LbGames.ItemsSource = Client.ShowAllLiveGames();
                }
                else
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<DbFault> fault) //DB exception has happened at the server.
            {
                if (fault.InnerException != null)
                    MessageBox.Show(
                        fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
            }
            catch (Exception ex) // all un know exceptions.
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                    MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
