using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for LoginWindow.xaml
    /// this window for login and connect to the server.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool _manualClosing = false; // when a manual closing create.
        public LoginWindow()
        {
            InitializeComponent();
        }

       /// <summary>
       /// this function is when click at the connect button.
       /// check at the DB and create a client.
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbUsername.Text) &&
                !string.IsNullOrEmpty(TbPassword.Password)) 
            {
                ConnectButton.Cursor = Cursors.Wait;
                var clientCallBack = new ClientCallBack(); // create a client callback.
                var client = new FourRowServiceClient(new InstanceContext(clientCallBack)); // create the client.

                string username = TbUsername.Text.Trim();
                var name = username;
                string password = TbPassword.Password.Trim();
                try
                {
                    
                    if (WaitingRoomWindow.CheckPingToServer(client)) 
                    {
                        client.Connect(username, HashValue(password)); // try to connect.
                        var waitingRoomWindow = new WaitingRoomWindow(clientCallBack) // create a waiting window..
                        {
                            Client = client,
                            Username = username,
                            Title = username + " Waiting Room"
                        };

                        _manualClosing = true; // manual closing is happened.
                        Close();
                        ConnectButton.Cursor = Cursors.Arrow;
                        waitingRoomWindow.Show();
                    }
                    else
                    {
                        MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
                catch (FaultException<UserExistsFault> fault) // user found at the DB at the server.
                {
                    MessageBox.Show(fault.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FaultException<UserNotExistsFault> fault) // user not found at the DB at the server.
                {
                    MessageBox.Show(fault.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FaultException<WrongPasswordFault> fault) // user wrong password error.
                {
                    MessageBox.Show(fault.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (TimeoutException) // timeout exceptions.
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex) // all un know exceptions.
                {
                    if(ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                        MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if(string.IsNullOrEmpty(TbUsername.Text))
                    MessageBox.Show("Please Fill Username!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Please Fill Password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// hash function for password read the DB validation.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashValue(string password)
        {
            using (SHA256 hashObject = SHA256.Create())
            {
                byte[] hashBytes = hashObject.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #region arrow, mouse enter/leave cursor

        private void RegisterLabel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }

        private void RegisterLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            RegisterLabel.Foreground = new SolidColorBrush(Colors.Blue);
            RegisterLabel.Cursor = Cursors.Hand;
        }

        private void RegisterLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            RegisterLabel.Foreground = new SolidColorBrush(Colors.Black);
            RegisterLabel.Cursor = Cursors.Arrow;
        }

        private void ConnectButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ConnectButton.Cursor = Cursors.Hand;
        }

        private void ConnectButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ConnectButton.Cursor = Cursors.Arrow;
        }

        #endregion

        /// <summary>
        /// exit environment function.
        /// </summary>
        /// <returns></returns>
        private bool Exit()
        {
            var result = MessageBox.Show(" Are You Sure You Want To Exit?",
                "Exit!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Environment.Exit(Environment.ExitCode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// exit check with bypass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_manualClosing)
            {
                if (!Exit())
                    e.Cancel = true;
            }
        }
    }
}
