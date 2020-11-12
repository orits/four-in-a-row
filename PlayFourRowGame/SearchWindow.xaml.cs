using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
    /// Interaction logic for SearchWindow.xaml
    /// this window for show all user info by: (UserName, NumberOfGames, NumberOfVictory, NumberOfLosses, NumberOfPoints)
    /// </summary>
    public partial class SearchWindow : Window
    {
        private bool _isTop10 = false; // is top10 mode.

        public SearchWindow(bool isTop10)
        {
            InitializeComponent();
            _isTop10 = isTop10;
        }

        public FourRowServiceClient Client { get; set; } // client ref/
        public string Username { get; set; } // user name.

        /// <summary>
        /// sort all result by username.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByUserNameMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                try
                {
                    var results = Client.SortAllRegisterUsers(Username, "UserName"); // ask form server for all user result games info.
                    LbRegisterUsers.ItemsSource = results;
                    if (results.Count == 0) // no result found.
                        NoValues();
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
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// sort all result by number of games.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByNumberOfGamesMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                try
                {
                    var results = Client.SortAllRegisterUsers(Username, "NumberOfGames");
                    LbRegisterUsers.ItemsSource = results;
                    if (results.Count == 0)
                        NoValues();
                }
                catch (FaultException<DbFault> fault)
                {
                    if (fault.InnerException != null)
                        MessageBox.Show(
                            fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// sort all result by number winning games.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByNumberOfVictoryMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                try
                {
                    var results = Client.SortAllRegisterUsers(Username, "NumberOfVictory");
                    LbRegisterUsers.ItemsSource = results;
                    if (results.Count == 0)
                        NoValues();
                }
                catch (FaultException<DbFault> fault)
                {
                    if (fault.InnerException != null)
                        MessageBox.Show(
                            fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// sort all result by number of losing games.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByNumberOfLossesMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                try
                {
                    var results = Client.SortAllRegisterUsers(Username, "NumberOfLosses");
                    LbRegisterUsers.ItemsSource = results;
                    if (results.Count == 0)
                        NoValues();
                }
                catch (FaultException<DbFault> fault)
                {
                    if (fault.InnerException != null)
                        MessageBox.Show(
                            fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// sort all result by sum of points of clients.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByNumberOfPointsMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client))
            {
                try
                {
                    var results = Client.SortAllRegisterUsers(Username, "NumberOfPoints");
                    LbRegisterUsers.ItemsSource = results;
                    if (results.Count == 0)
                        NoValues();
                }
                catch (FaultException<DbFault> fault)
                {
                    if (fault.InnerException != null)
                        MessageBox.Show(
                            fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// this function called when window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isTop10) // regular mod.
            {
                var results = Client.SortAllRegisterUsers(Username, "UserName");
                results = results.OrderByDescending(r => r.UserName).ToList();
                LbRegisterUsers.ItemsSource = results;
                MessageBox.Show("Please Select Option to Sort using - Sort By", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else // top10 mode
            {
                DockPanel.Visibility = Visibility.Hidden;
                if (WaitingRoomWindow.CheckPingToServer(Client))
                {
                    try
                    {
                        var results = Client.SortAllRegisterUsers(Username, "NumberOfVictory");
                        results = results.OrderByDescending(r => r.NumberOfVictory).Take(10).ToList();
                        LbRegisterUsers.ItemsSource = results;
                        if (results.Count == 0)
                            NoValues();
                    }
                    catch (FaultException<DbFault> fault) //DB exception has happened at the server.
                    {
                        if (fault.InnerException != null) 
                            MessageBox.Show(
                                fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
                    }
                    catch (TimeoutException) // timeout exceptions.
                    {
                        MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex) // all un know exceptions.
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                            MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private static void NoValues()
        {
            MessageBox.Show("No Values To Show!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
