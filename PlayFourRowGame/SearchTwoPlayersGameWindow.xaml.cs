using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SearchTwoPlayersGameWindow.xaml
    /// this window is for search two player that play.
    /// </summary>
    public partial class SearchTwoPlayersGameWindow : Window
    {
        public SearchTwoPlayersGameWindow()
        {
            InitializeComponent();
        }

        public FourRowServiceClient Client { get; set; } // client ref.

        /// <summary>
        /// this function for search two selected player for the list box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var tempUserName = new List<string>(); // selected two users name..
            LbGames.ItemsSource = null;
            if (LbAllClients.SelectedItems.Count < 2)
            {
                MessageBox.Show("Please Select Two Players!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var selectedItem in LbAllClients.SelectedItems) // save all the users.
            {
                var userName = selectedItem as string;
                if (userName != null)
                {
                    tempUserName.Add(userName);
                }
            }

            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                var results = Client.ShowAllHistoryGamesTwoPlayers(tempUserName); // ask for records of the two players.
                LbGames.ItemsSource = results;
                if (results == null) // check if records has found.
                {
                    TbUsersWinnerPercent.Text = "No Much Has Found!";
                    TbUsersWinnerPercent.Foreground = new SolidColorBrush(Colors.Red);
                }
                else // parse the found records to view.
                {
                    var totalGame = results.Count;
                    var winUserOne = results.Where(h => h.WinUserName == tempUserName[0]).ToList().Count;
                    var winUserTwo = results.Where(h => h.WinUserName == tempUserName[1]).ToList().Count;
                    TbUsersWinnerPercent.Foreground = new SolidColorBrush(Colors.Black);

                    var ratioUserOne = ((double)winUserOne / totalGame) * 100;
                    var strRatioOne = $"{ratioUserOne:0.00}";
                    var ratioUserTwo = ((double)winUserTwo / totalGame) * 100;
                    var strRatioTwo = $"{ratioUserTwo:0.00}";
                    TbUsersWinnerPercent.Text = $"{tempUserName[0]} Win %: {strRatioOne}, {tempUserName[1]} Win %: {strRatioTwo}";
                }
            }
        }

        #region arrow, mouse enter/leave cursor

        private void SearchButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SearchButton.Cursor = Cursors.Hand;
        }

        private void SearchButton_MouseLeave(object sender, MouseEventArgs e)
        {
            SearchButton.Cursor = Cursors.Arrow;
        } 

        #endregion

        private void SearchTwoPlayersGameWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //
        }

        /// <summary>
        /// this function called when a click has was seen at the "LbAllClients".
        /// </summary>
        private void LbAllClients_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LbGames.ItemsSource = null;
            TbUsersWinnerPercent.Text = null;
            if (LbAllClients.SelectedItems.Count == 3) // fix if more the two player selected.
            {
                MessageBox.Show("Please Select Only 2 Players!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                var a = LbAllClients.SelectedItems[0];
                var b = LbAllClients.SelectedItems[1];
                LbAllClients.SelectedItems.Clear();
                LbAllClients.SelectedItems.Add(a);
                LbAllClients.SelectedItems.Add(b);
            }
        }

        /// <summary>
        /// this function called when the window is loaded.
        /// </summary>
        private void SearchTwoPlayersGameWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WaitingRoomWindow.CheckPingToServer(Client))
                {
                    LbAllClients.ItemsSource = Client.GetAllRegisterClients(); // ask & get all register users for the server.
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
                MessageBox.Show(
                    ex.Message + " ##\n##\n" + ex.InnerException.Message);
            }
        }
    }
}
