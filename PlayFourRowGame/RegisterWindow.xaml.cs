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
    /// Interaction logic for RegisterWindow.xaml
    /// this window for register to the four row game.
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private string _emojiName; // emoji name form emoji folder.

        private bool _manualClosing = false; // when a manual closing create.
        public RegisterWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// this function called  when click the register button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            bool hasChangeDb = false; // when DB has update. at the end of register.

            string password = TbPassword.Password.Trim();
            if (!string.IsNullOrEmpty(TbUsername.Text) &&
                !string.IsNullOrEmpty(TbPassword.Password) &&
                !string.IsNullOrEmpty(TbEmojiName.Text))
            {
                var clientCallBack = new ClientCallBack(); // temp callback obj.
                var client = new FourRowServiceClient(new InstanceContext(clientCallBack)); // client for this moment.
                var userName = TbUsername.Text.Trim();
                var emojiName = TbEmojiName.Text;
                try
                {
                    if(WaitingRoomWindow.CheckPingToServer(client)) // try to register at the server.
                        hasChangeDb = client.Register(userName, LoginWindow.HashValue(password), emojiName);
                }
                catch (FaultException<UserExistsFault> fault)  // user exist at the DB (key-username).
                {
                    MessageBox.Show(fault.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FaultException<DbFault> fault) // DB exception has happened at the server.
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

                if (hasChangeDb) // notify about the DB update.
                {
                    MessageBox.Show("Register Success ✔️, Database UpDate!",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    _manualClosing = true; // for moving the waiting room without ask if want the exit.
                    Close();
                }
            }
            else // no all field are fill.
            {
                if (!string.IsNullOrEmpty(TbUsername.Text))
                    MessageBox.Show("Please Fill Username!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if(!string.IsNullOrEmpty(TbPassword.Password))
                    MessageBox.Show("Please Fill Password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Please Browser a Icon!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// this function when click the browse button the select emoji image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmojiSelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            var emojiWindow = new EmojIWindow();
            emojiWindow.ShowDialog();
            _emojiName = emojiWindow.EmojiName;
            TbEmojiName.Text = _emojiName;
        }

        #region arrow, mouse enter/leave cursor

        private void RegisterButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            RegisterButton.Cursor = Cursors.Hand;
        }

        private void RegisterButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            RegisterButton.Cursor = Cursors.Arrow;
        }

        private void OnMouseEnter_MouseEnter(object sender, MouseEventArgs e)
        {
            RegisterButton.Cursor = Cursors.Hand;
        }

        private void OnMouseEnter_MouseLeave(object sender, MouseEventArgs e)
        {
            RegisterButton.Cursor = Cursors.Arrow;
        }

        #endregion

        /// <summary>
        /// exit function.focus back to login window.
        /// </summary>
        /// <returns></returns>
        private bool Exit()
        {
            var result = MessageBox.Show(" Are You Sure You Want To Stop Register?",
                "Exit!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// exit check with bypass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_manualClosing)
            {
                if (!Exit()) 
                    e.Cancel = true;
            }
        }

       
    }
}
