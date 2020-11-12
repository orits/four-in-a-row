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
    /// Interaction logic for SummeryUserGameWindows.xaml
    /// this window is for summery user Game history.
    /// </summary>
    public partial class SummeryUserGameWindows : Window
    {
        public SummeryUserGameWindows()
        {
            InitializeComponent();
        }

        public FourRowServiceClient Client { get; set; } // client ref.
        public string Username { get; set; } // username

        /// <summary>
        /// this function will happened when window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SummeryUserGameWindows_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                try
                { //ask for server records about the player.
                    var result = Client.ShowAllPlayerHistoryGames(Username); 
                    var totalGame = result.Item1.Count;
                    var winGame = result.Item2.NumberOfVictory;
                    if (totalGame > 0) // no game founds 
                    {
                        var ratio = ((double)winGame / totalGame) * 100;
                        var strRatio = $"{ratio:0.00}";
                        TbSummeryDetails.Text = $"{Username} Win: {strRatio}%, Number Of Games: {result.Item2.NumberOfGames} \n" +
                                                $"Number Of Victory: {result.Item2.NumberOfVictory}, Number Of Losses: {result.Item2.NumberOfLosses} \n" +
                                                $"Number Of Points: {result.Item2.NumberOfPoints}";
                    }
                    else // history games has found!
                    {
                        TbSummeryDetails.Text = $"{Username} Win: 0%, Number Of Games: {result.Item2.NumberOfGames} \n" +
                                                $"Number Of Victory: {result.Item2.NumberOfVictory}, Number Of Losses: {result.Item2.NumberOfLosses} \n" +
                                                $"Number Of Points: {result.Item2.NumberOfPoints}";
                    }
                    LbGames.ItemsSource = result.Item1;
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
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
