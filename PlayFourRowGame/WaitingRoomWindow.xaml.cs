using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using PlayFourRowGame.FourRowServiceReference;

namespace PlayFourRowGame
{
    /// <summary>
    /// Interaction logic for WaitingRoomWindow.xaml
    /// this window is for waiting room.
    /// </summary>
    public partial class WaitingRoomWindow : Window
    {
        private bool _isValidInvite = false; // invite is still at rang of 60 sec.

        private WaitingForInviteResultWindow waitingForInviteResultWindow; // WaitingForInviteResultWindow ref.

        private DispatcherTimer _timer = new DispatcherTimer(); // timer for update waiting clients.

        private DispatcherTimer _timerValidInvite; // invite vaild  timer.

        private ClientCallBack _clientCallBack; // callback ref.

        // c'tor.
        public WaitingRoomWindow(ClientCallBack clientCallBack)
        {
            InitializeComponent();
            _clientCallBack = clientCallBack;
            _clientCallBack.InviteToPlay = InvitedToPlay;
            _clientCallBack.ReplyInviteToPlay = ReplyInvitedToPlay;
            InviteUserToPlayButton.IsEnabled = false;
        }

        public FourRowServiceClient Client { get; set; } // client ref.

        public string Username { get; set; } // username.

        /// <summary>
        /// start game function between two clients.
        /// </summary>
        /// <param name="username">username one</param>
        /// <param name="fromUserName">username two</param>
        /// <param name="isStatFirst">is username one start first</param>
        private void StartGame(string username, string fromUserName, bool isStatFirst)
        {
            var fourRowGameWindow = new FourRowGameWindow(_clientCallBack, username, fromUserName, isStatFirst);
            fourRowGameWindow.Client = Client;
            _timer.Stop(); // stop get list of waiting clients.
            _isValidInvite = false; // invite is not valid.
            _timerValidInvite.Stop(); // invite timer is stop.
            ExitButton.IsEnabled = true; // exit is clickble.
            Hide(); // hide waiting room while playing.
            LbWaitingUsers.ItemsSource = null;
            fourRowGameWindow.ShowWaitingRoomAfterEndGame = ShowAndReadyAfterEndGame; // singup to func after end game.
            fourRowGameWindow.Show();
        }

        /// <summary>
        /// this function is called when this client is invited by other client, and want to play.
        /// </summary>
        /// <param name="fromUserName"> the invited username </param>
        private void InvitedToPlay(string fromUserName)
        {
            InviteUserToPlayButton.IsEnabled = false;
            _timer.Stop(); //stop client waiting timer.

            _isValidInvite = true; // invite is valid for 60 sec.
            ValidInviteStartTimer(); // start count 60 sec.
            ExitButton.IsEnabled = false; // exit button lock.

            var result = MessageBox.Show(Username + " You Are Invited To Play!\nDid You Want To Play With: " + fromUserName + " ?",
                Username + " Invite Is Valid Only For 60 Seconds Only!", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (_isValidInvite) // invite is valid.
            {
                try
                {
                    if (result == MessageBoxResult.Yes) // you want to play with "fromUserName"
                    {
                        Client.ReplyToInviteOtherPlayer(Username, fromUserName, true); // ask for server to reply yes to "fromUserName"
                        Client.StartGame(fromUserName, Username); // inform the server the two players are started to play.
                        StartGame(Username, fromUserName, false);  // start game, client play second.
                    }
                    else
                    {
                        Client.ReplyToInviteOtherPlayer(Username, fromUserName, false); // ask for server to reply no to "fromUserName"
                        _isValidInvite = false; // invite is not valid.
                        _timerValidInvite.Stop(); // invite timer is stop (60 sec).
                        ExitButton.IsEnabled = true; // exit is clickble.
                        _timer.Start(); // start timer for waiting clients.
                    }
                }
                catch (FaultException<ExceptionFault> fault) // exceptions form the server that catch.
                {
                    MessageBox.Show(fault.Message + "\n" + "Type: " + fault.GetType().ToString() +
                                    "\nInner exception: " + fault.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (TimeoutException) // Timeout..
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _timer.Stop();
                }
                catch (Exception ex) // all un know exceptions.
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message + "\n" + "Type: " + ex.GetType().ToString() +
                                        "\nInner exception: " + ex.InnerException.Message,"Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else // client select yes or no but the invite isn't valid (more then 60 sec)
            {
                if (result == MessageBoxResult.Yes)
                    MessageBox.Show(Username + " The Invite Is No Longer Valid.\nThe Invite Valid For 60 Seconds Only!",
                    Username + " Invite Time Is Passed!!", MessageBoxButton.OK, MessageBoxImage.Stop);

                InviteUserToPlayButton.IsEnabled = true;
                _timer.Start();
            }
        }

        /// <summary>
        /// this function is called after the server is reply to your invite from the other client.
        /// </summary>
        /// <param name="fromUserName"> the other client username</param>
        /// <param name="reply">did the the other client accept or decline </param>
        private void ReplyInvitedToPlay(string fromUserName, bool reply)
        {

            if (reply) // let play he say....
            {
                waitingForInviteResultWindow.StartTimerAfterReply(true);
                StartGame(Username, fromUserName, true);
            }
            else // no playing will be here!
            {
                if(_isValidInvite)
                    waitingForInviteResultWindow.StartTimerAfterReply(false);

                ExitButton.IsEnabled = true;
                _isValidInvite = false;
                InviteUserToPlayButton.IsEnabled = true;
                _timer.Start();
            }
        }

        /// <summary>
        /// this function is called when client click to invite other client.
        /// </summary>
        private void InviteUserToPlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (LbWaitingUsers.SelectedItem != null)
            {
                _timer.Stop(); // stop timer get waiting clients.
                _isValidInvite = true; // invite is valid.
                ValidInviteStartTimer(); // start timer for 60 sec.
                var toUserName = LbWaitingUsers.SelectedItem as string;
                InviteUserToPlayButton.IsEnabled = false; // lock invite button.

                waitingForInviteResultWindow = new WaitingForInviteResultWindow(Username, toUserName);
                waitingForInviteResultWindow.Title = "Invite Is Valid For 60 Seconds Only, Exit Is Locked!";

                try
                {
                    waitingForInviteResultWindow.TimeOutAction = () =>
                    {
                        if (CheckPingToServer(Client)) // if invite timeout happened, server send to this client "no".
                            Client.ReplyToInviteOtherPlayer(toUserName, Username, false);
                    };
                    waitingForInviteResultWindow.Show();

                    if (CheckPingToServer(Client))
                        Client.InviteOtherPlayer(Username, toUserName);
                }
                catch (TimeoutException) // Timeout..
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _timer.Stop();
                }
                catch (FaultException<ExceptionFault> fault) // exceptions that catch at the server side.
                {
                    MessageBox.Show(fault.Message + "\n" + "Type: " + fault.GetType().ToString() +
                                    "\nInner exception: " + fault.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex) // all un know exceptions.
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message + "\n" + "Type: " + ex.GetType().ToString() +
                                        "\nInner exception: " + ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                } 
            }
            else
            {
                MessageBox.Show("Please Select Waiting User!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// this function is static for use at other windiws.
        /// need to check ping and need to return false when no link to server after time.
        /// this function is danny kotler code, with my extra.
        /// </summary>
        /// <param name="client">Client ref</param>
        /// <returns></returns>
        public static bool CheckPingToServer(FourRowServiceClient client)
        {
            if (client.Endpoint.Binding != null)
            {
                var ts = client.Endpoint.Binding.ReceiveTimeout;
                var time = new TimeSpan(0, 0, 10);
                client.Endpoint.Binding.CloseTimeout = time;
                client.Endpoint.Binding.OpenTimeout = time;
                client.Endpoint.Binding.ReceiveTimeout = time;
                client.Endpoint.Binding.SendTimeout = time;

                if (client.Ping())
                {
                    if (client.Endpoint.Binding != null) client.Endpoint.Binding.ReceiveTimeout = ts;
                    return true;
                }

                if (client.Endpoint.Binding != null) client.Endpoint.Binding.ReceiveTimeout = ts;
            }

            return false;
        }

        /// <summary>
        /// this func is wakeup with timer, and ask form server list of live clients, that can play.
        /// </summary>
        private void UpdateConnectedClients()
        {
            try
            {
                if (CheckPingToServer(Client))
                {
                    List<string> liveClients = Client.GetClients();
                    liveClients.Remove(Username);
                    if (liveClients.Count == 0)
                    {
                        InviteUserToPlayButton.IsEnabled = false; // lock button if list empty.
                        LbWaitingUsers.ItemsSource = null;
                    }
                    else
                    {
                        InviteUserToPlayButton.IsEnabled = true; // not lock button if list not empty.
                        LbWaitingUsers.ItemsSource = liveClients;
                    }
                }
                else
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _timer.Stop();
            }
        }


        /// <summary>
        /// this function is called when window is loaded, start the UpdateConnectedClients timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromSeconds(4);
            _timer.Tick += delegate { UpdateConnectedClients(); };
            _timer.Start();
        }

        /// <summary>
        /// this function is for safe exit for the game.
        /// </summary>
        /// <returns></returns>
        private bool Exit()
        {
            if (!_isValidInvite) // invite mod, then lock.
            {
                var result = MessageBox.Show(Username + " Are You Sure You Want To Exit?",
                    "Exit!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (CheckPingToServer(Client))
                        {
                            _timer.Stop();
                            Client.Disconnect(Username);
                        }
                        else
                        {
                            MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (TimeoutException) // Timeout..
                    {
                        MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex) // all un know exceptions.
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                            MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            MessageBox.Show(ex.Message);
                    }
                    Environment.Exit(Environment.ExitCode); // close all windows...
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// exit function with bypass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitingRoomWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!Exit())
                e.Cancel = true;
        }


        /// <summary>
        /// exit button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        /// <summary>
        /// this function is called when waiting client is selected and the "SummeryOfUserGamesButton" is clickble.
        /// </summary>
        private void LbWaitingUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var user = LbWaitingUsers.SelectedValue as string;
            if (user != null)
            {
                SummeryOfUserGamesButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// this function is called when click "AllRegistersUsersButton".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllRegistersUsersButton_OnClick(object sender, RoutedEventArgs e)
        {
            var searchWindow = new SearchWindow(false);
            searchWindow.Username = Username;
            searchWindow.Client = Client;
            searchWindow.Show();
           
        }

        /// <summary>
        /// this function is called when click "AllHistoryGamesButton".
        /// </summary>
        private void AllHistoryGamesButton_OnClick(object sender, RoutedEventArgs e)
        {
           var searchGameWindow = new SearchGameWindow(true);
           searchGameWindow.Title = "All History Games";
           searchGameWindow.Username = Username;
           searchGameWindow.Client = Client;
           searchGameWindow.Show();

        }

        /// <summary>
        /// this function is called when click "AllLiveGamesButton".
        /// </summary>
        private void AllLiveGamesButton_OnClick(object sender, RoutedEventArgs e)
        {
            var searchGameWindow = new SearchGameWindow(false);
            searchGameWindow.Title = "All Live Games";
            searchGameWindow.Username = Username;
            searchGameWindow.Client = Client;
            searchGameWindow.Show();
        }

        /// <summary>
        /// this function is called when click "TwoPlayersGamesButton".
        /// </summary>
        private void TwoPlayersGamesButton_OnClick(object sender, RoutedEventArgs e)
        {
            var searchTwoPlayersGameWindow = new SearchTwoPlayersGameWindow();
            searchTwoPlayersGameWindow.Client = Client;
            searchTwoPlayersGameWindow.Show();
        }

        /// <summary>
        /// this function is called when click "SummeryOfUserGamesButton".
        /// </summary>
        private void SummeryOfUserGamesButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedUser = LbWaitingUsers.SelectedItem as string;
            var summeryUserGameWindows = new SummeryUserGameWindows();
            summeryUserGameWindows.Title = selectedUser + " Summery Of Games";
            summeryUserGameWindows.Username = selectedUser;
            summeryUserGameWindows.Client = Client;
            summeryUserGameWindows.Show();
        }

        /// <summary>
        /// this function is called when click "Top10PlayersButton".
        /// </summary>
        private void Top10PlayersButton_OnClick(object sender, RoutedEventArgs e)
        {
            var searchWindow = new SearchWindow(true);
            searchWindow.Username = Username;
            searchWindow.Client = Client;
            searchWindow.Title += " Top 10 Player!";
            searchWindow.Show();
        }


        /// <summary>
        /// this function is started the timer for the 60 sec valid invite.
        /// </summary>
        private void ValidInviteStartTimer()
        {
            ExitButton.IsEnabled = false;
            _timerValidInvite = new DispatcherTimer();
            _timerValidInvite.Interval = TimeSpan.FromSeconds(60);
            _timerValidInvite.Tick += delegate { InviteValidTimerTick(); };
            _timerValidInvite.Start();
        }


        /// <summary>
        /// this function is called when 60 sec has pass.
        /// </summary>
        private void InviteValidTimerTick()
        {
            ExitButton.IsEnabled = true; // can exit now.
            _isValidInvite = false; // invite is invalid.
            _timerValidInvite.Stop();
            _timerValidInvite.Tick -= delegate { InviteValidTimerTick(); };
            _timer.Start(); //start client waiting timer.
        }


        /// <summary>
        /// this function is called after exit from the game window after a game.
        /// </summary>
        /// <param name="opponentUserName"> opponent user name that client play with </param>
        private void ShowAndReadyAfterEndGame(string opponentUserName)
        {
            if (CheckPingToServer(Client))
            {
                Client.BackToWaitingClients(Username, opponentUserName); // update the server that the play between player is over.
                _timer.Start(); // start to waiting clients.
                ExitButton.IsEnabled = true; // can exit no.
                Show(); // display again the waiting window.
            }
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region mouse enter/leave cursor

        private void AllHistoryGameButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AllHistoryGameButton.Cursor = Cursors.Hand;
        }

        private void AllHistoryGameButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AllHistoryGameButton.Cursor = Cursors.Arrow;
        }

        private void AllRegistersUsersButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AllRegistersUsersButton.Cursor = Cursors.Arrow;
        }

        private void AllRegistersUsersButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AllRegistersUsersButton.Cursor = Cursors.Hand;
        }

        private void AllLiveGamesButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AllLiveGamesButton.Cursor = Cursors.Hand;
        }

        private void AllLiveGamesButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AllLiveGamesButton.Cursor = Cursors.Arrow;
        }

        private void TwoPlayersGamesButton_MouseEnter(object sender, MouseEventArgs e)
        {
            TwoPlayersGamesButton.Cursor = Cursors.Hand;
        }

        private void TwoPlayersGamesButton_MouseLeave(object sender, MouseEventArgs e)
        {
            TwoPlayersGamesButton.Cursor = Cursors.Arrow;
        }

        private void SummeryOfUserGamesButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SummeryOfUserGamesButton.Cursor = Cursors.Hand;
        }

        private void SummeryOfUserGamesButton_MouseLeave(object sender, MouseEventArgs e)
        {
            SummeryOfUserGamesButton.Cursor = Cursors.Arrow;
        }

        private void Top10PlayersButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Top10PlayersButton.Cursor = Cursors.Hand;
        }

        private void Top10PlayersButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Top10PlayersButton.Cursor = Cursors.Arrow;
        }

        private void InviteUserToPlayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            InviteUserToPlayButton.Cursor = Cursors.Hand;
        }

        private void InviteUserToPlayButton_MouseLeave(object sender, MouseEventArgs e)
        {
            InviteUserToPlayButton.Cursor = Cursors.Arrow;
        }

        private void ExitButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ExitButton.Cursor = Cursors.Hand;
        }

        private void ExitButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ExitButton.Cursor = Cursors.Arrow;
        }
        #endregion

    }
}
