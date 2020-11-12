using System;
using System.Collections.Generic;
using System.Linq;
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

namespace PlayFourRowGame
{
    /// <summary>
    /// Interaction logic for WaitingForInviteResultWindow.xaml
    /// this window is for waiting reply after send a invite to other client.
    /// </summary>
    public partial class WaitingForInviteResultWindow : Window
    {

        private int _countToCloseAfterReply = 6; // const number of seconds to close after reply (yes,no)

        private int _countToCloseNoReply = 6; // const number of seconds to close timeout happened.

        private string _message; // message to display/

        private string _fromUserName, _toUserName; // invited user name.

        private DispatcherTimer _timer; // timer of timeout (invite is valid timer)

        public Action TimeOutAction;

        // c'tor.
        public WaitingForInviteResultWindow(string fromUserName, string toUserName)
        {
            InitializeComponent();
            _message =  $"{fromUserName} Wait For Invite Reply Form {toUserName}....";
            TextBlockLabel.Text = _message;
            _fromUserName = fromUserName;
            _toUserName = toUserName;
        }

        /// <summary>
        /// this function is called when window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartCloseTimer();
        }

        /// <summary>
        /// this func is called when the client get reply form the server (yes or no)
        /// and will started a timer for 6 sec and close the windows.
        /// </summary>
        /// <param name="reply"></param>
        public void StartTimerAfterReply(bool reply)
        {
            _timer.Stop();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.2);
            timer.Tick += delegate { TimerTickReply(timer, reply); };
            timer.Start();
        }

        /// <summary>
        /// this func will count 5,4,..,1,0, and will show the client the reply (yes, no)
        /// and after the time will close.
        /// </summary>
        private void TimerTickReply(DispatcherTimer timer, bool reply)
        {
            if (--_countToCloseAfterReply <= 5)
            {
                if(reply)
                    TextBlockLabel.Text = _message + $"\n{_toUserName} Accept Your Invite. \nThis Window Will Close In: {_countToCloseAfterReply} Seconds!";
                else
                    TextBlockLabel.Text = _message + $"\n{_toUserName} Decline Your Invite. \nThis Window Will Close In: {_countToCloseAfterReply} Seconds";
                if (_countToCloseAfterReply == -1)
                {
                    timer.Stop();
                    Close();
                }
            }
        }

        /// <summary>
        /// the function the count 60 sec to valid invite.
        /// </summary>
        private void StartCloseTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(60);
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        /// <summary>
        /// this function is start after timeout happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            DispatcherTimer timer = (DispatcherTimer) sender;
            timer.Tick -= TimerTick;
            StartTimerBeforeTimeout();
        }

        /// <summary>
        /// this function called after timeout for close and to display the timeout info.
        /// </summary>
        private void StartTimerBeforeTimeout()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.2);
            timer.Tick += delegate { TimerTickBeforeTimeout(timer); };
            timer.Start();
        }

        /// <summary>
        /// this function is count down 5,4,...,1,0, and close while display the timeout.
        /// </summary>
        /// <param name="timer"></param>
        private void TimerTickBeforeTimeout(DispatcherTimer timer)
        {
            if (--_countToCloseNoReply <= 5)
            {
                TextBlockLabel.Foreground = new SolidColorBrush(Colors.Red);
                TextBlockLabel.Text = $"\nTimeout!!,\n{_toUserName} Didn't Reply And The Invite Is No Longer Valid!\nThis Window Will Close In: {_countToCloseNoReply} Seconds";
                if (_countToCloseNoReply == -1)
                {
                    timer.Stop();
                    TimeOutAction();
                    Close();
                }
            }
        }
    }
}
