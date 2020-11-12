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
    /// Interaction logic for FourRowGameWindow.xaml
    /// this window for game board and info about the game.
    /// </summary>
    public partial class FourRowGameWindow : Window
    {
        private int _pos = 0; // position of canves.

        private int _ticks = 0; // counter of ticks

        private DispatcherTimer _timer; // timer of the game.

        private Game _fourRowGame; // the logic of the for row game.

        private readonly bool _isStartFirst; // true when client start first, client invite the opponent.

        private readonly string _userName; // client user name.

        private readonly string _opponentUserName; // opponent user name.

        private int _opponentScore; // opponent score.

        private bool _endGame; // true when game ended (full game or exit).

        private readonly SolidColorBrush _myColorBrush; // client color disk.

        private readonly SolidColorBrush _opponentColorBrush; // opponent color disk.

        private ClientCallBack _clientCallBack; // client callBack ref.

        // c'tor.
        public FourRowGameWindow(ClientCallBack clientCallBack, string userName, string opponentUserName, bool isStatFirst)
        {
            InitializeComponent();
            _isStartFirst = isStatFirst;
            _userName = userName;
            _opponentUserName = opponentUserName;

            _clientCallBack = clientCallBack;
            _clientCallBack.UpdateGame = OpponentMoveUpdate;
            _clientCallBack.EndGame = EndGame;
            _clientCallBack.OpponentDisconnect = NotifyOpponentDisconnect;

            if (_isStartFirst) // first play.
            {
                _fourRowGame = new Game('R', 'Y', _isStartFirst);
                _myColorBrush = new SolidColorBrush(Colors.DarkRed);
                _opponentColorBrush = new SolidColorBrush(Colors.DarkGoldenrod);
                MyTurn();
            }
            else // second play.
            {
                _fourRowGame = new Game('Y', 'R', _isStartFirst);
                _myColorBrush = new SolidColorBrush(Colors.DarkGoldenrod);
                _opponentColorBrush = new SolidColorBrush(Colors.DarkRed);
                NotMyTurn();
            }
        }

        public FourRowServiceClient Client { get; set; } // Client Obj for tunnel to the "server".


        public Action<string> ShowWaitingRoomAfterEndGame; // event that called after game end (good or bad).


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WaitingRoomWindow.CheckPingToServer(Client)) // show clients images
            {
                var imageClients = Client.GetImageClients(_userName, _opponentUserName);
                LoadImage(imageClients[0], true);
                LoadImage(imageClients[1], false);
            }
            else
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TbUserNameOne.Text = "(You) " + _userName  + ":";
            TbUserNameTwo.Text = _opponentUserName + ":";

            Title = $"{_userName} Board Game";

            CurrentPlayer.Fill = _isStartFirst ? _myColorBrush : _opponentColorBrush; // starting disk color...

            StartTimerTick(); // start timer.
        }


        private void LoadImage(string image, bool isMe)
        {
            string url = "emoji/" + image;

            if (isMe) //client image.
                UserOneImage.Source = new BitmapImage(new Uri(@url, UriKind.Relative));
            else    //opponent image.
                UserTwoImage.Source = new BitmapImage(new Uri(@url, UriKind.Relative));
        }

        /// <summary>
        /// click at the canvas blue board.
        /// </summary>
        private void CanvasBlueGameBoard_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_endGame) // game still running...
            {
                if (_fourRowGame.IsMyTurn) // client turn to make his move.
                {
                    Point p = e.GetPosition(CanvasBlueGameBoard);
                    Ball newBall = new Ball(p);
                    newBall.El.Fill = _myColorBrush;
                    var move = GetPosAndRowColumn(newBall);
                    _pos = move.Pos;
                    move.Point = p;

                    if (_pos != -1) 
                    {
                        MovePlayed(move, newBall); // make his move.
                    }
                    else
                    {
                        MessageBox.Show(_userName + " Your Move Is Illegal!, Please Try Again", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(_userName + " Wait For Your Turn!", "Info", MessageBoxButton.OK, MessageBoxImage.Stop);
                } 
            }
            else
            {
                MessageBox.Show(_userName + " The Game Is Over, Please Exit!", "Info", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        /// <summary>
        /// play the played move, graphic & logic
        /// </summary>
        private void MovePlayed(Move move, Ball newBall)
        {
            if (_fourRowGame.Move(move.Column)) // make his move at the game model/logic.
            {
                try
                {
                    if (_fourRowGame.IsWin) // check win & report accordingly.
                    {
                        _endGame = true;
                        move.WinningRowDisks = _fourRowGame.WinningRowDisks; // set the winning row, for the nice display.
                        if (WaitingRoomWindow.CheckPingToServer(Client)) // report client move to the server.
                        {
                            Client.ReportMove(_userName, move, _opponentUserName, MoveResult.YouWon, _fourRowGame.Score);
                        }
                        else
                        {
                            MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            _endGame = true;
                        }
                    }
                    else if (_fourRowGame.IsTie) // only the invited client can call this, because hi played last and the board is full now.  
                    {
                        _endGame = true;
                        if (WaitingRoomWindow.CheckPingToServer(Client))
                        {
                            Client.ReportMove(_userName, move, _opponentUserName, MoveResult.Draw, _fourRowGame.Score);
                        }
                        else
                        {
                            MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            _endGame = true;
                        }
                    }
                    else
                    {
                        if (WaitingRoomWindow.CheckPingToServer(Client)) // report the move, game still playing.
                        {
                            Client.ReportMove(_userName, move, _opponentUserName, MoveResult.NotYourTurn, _fourRowGame.Score);
                        }
                        else
                        {
                            MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            _endGame = true;
                        }
                    }

                    RegisterMouseToBall(move.Column, newBall); // fill Ellipse can be "clicked". 
                    SmoothBallOnTheBoard(newBall); // show graphic at the screen.
                    if (_fourRowGame.IsWin) // show winning graphic at the screen.
                        ShowWinningRowDisks(_fourRowGame.WinningRowDisks, true);
                    TbUserOneScore.Text = "Score: " + _fourRowGame.Score.ToString(); // update my score.

                    if (!_endGame) // game didn't end yet.
                    {
                        CurrentPlayer.Fill = _opponentColorBrush; // set the opponent color disk/ball.
                        NotMyTurn(); // update turn info.
                    }

                    else // game ended (win, tie)
                    {
                        if (WaitingRoomWindow.CheckPingToServer(Client))
                        {
                            Client.EndGame(_userName, _fourRowGame.Score, _opponentUserName, _opponentScore);
                        }
                        else
                        {
                            MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            _endGame = true;
                        }
                        
                        TbTurn.Text = "The Game Ended, Please Exit!";
                        TbTurn.Foreground = new SolidColorBrush(Colors.DarkBlue);
                        _timer.Stop(); // stop game timer.

                        if(_fourRowGame.IsWin) // display winning message.
                            MessageBox.Show(_userName + " Win The Game!!", "Winner!!!", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
                catch (FaultException<OpponentDisconnectedFault> fault) // when opponent disconnect while playing.
                {
                    EndGame(fault.Detail.Details); // end game.
                    TbUserOneScore.Text = "Score: 1000";
                    TbUserTwoScore.Text = "Score: 0";
                }
                catch (FaultException<DbFault> fault) // DB exception that happened at the server while try to update DB.
                {
                    if (fault.InnerException != null)
                        MessageBox.Show(
                            fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message);
                }
                catch (TimeoutException) // Timeout..
                {
                    MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _endGame = true;
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


        /// <summary>
        /// this function manage the ball/disk display and movement.
        /// </summary>
        /// <param name="ball">the ball</param>
        /// <returns></returns>
        private Move GetPosAndRowColumn(Ball ball)
        {
            int col;
            if (ball.X < 60)
                col = 0;
            else if (ball.X < 150)
                col = 1;
            else if (ball.X < 245)
                col = 2;
            else if (ball.X < 340)
                col = 3;
            else if (ball.X < 435)
                col = 4;
            else if (ball.X < 520)
                col = 5;
            else
                col = 6;

            int row = 0, rowPos = -1;

            for (int i = 5; i >= 0; i--)
            {
                if (_fourRowGame.IsEmptyCell(i, col))
                {
                    row = i;
                    rowPos = i;
                    break;
                }
            }

            switch (rowPos)
            {
                case 0:
                    rowPos = 18;
                    break;
                case 1:
                    rowPos = 93;
                    break;
                case 2:
                    rowPos = 168;
                    break;
                case 3:
                    rowPos = 243;
                    break;
                case 4:
                    rowPos = 318;
                    break;
                case 5:
                    rowPos = 396;
                    break;
            }

            return new Move
            {
                Pos = rowPos,
                Row = row,
                Column = col
            };
        }

        /// <summary>
        /// this function will move the ball at the screen
        /// </summary>
        /// <param name="obj"> the ball as obj</param>
        private void MoveBall(object obj)
        {
            Ball ball = obj as Ball; // send as obj.
            ball.Y = 14;
            while (ball.Y != _pos)
            { 
                Dispatcher.Invoke((Action) (() =>
                {
                    if (ball.X < 70)
                    {
                        ball.X = 15;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }
                    else if (ball.X < 160)
                    {
                        ball.X = 114;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }
                    else if (ball.X < 250)
                    {
                        ball.X = 206;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }
                    else if (ball.X < 340)
                    {
                        ball.X = 298;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }
                    else if (ball.X < 430)
                    {
                        ball.X = 392;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }
                    else if (ball.X < 520)
                    {
                        ball.X = 488;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }
                    else
                    {
                        ball.X = 580;
                        ball.Y += ball.YSpeed;
                        if (ball.Y + ball.YSpeed > _pos)
                        {
                            ball.Y = _pos;
                        }
                    }

                    Canvas.SetTop(ball.El, ball.Y);
                    Canvas.SetLeft(ball.El, ball.X);
                }));
                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// this function for after a disk full, that the client can click at the disk for more disk at this column.
        /// full ellipse like empty.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="newBall"></param>
        private void RegisterMouseToBall(int column, Ball newBall)
        {
            switch (column) 
            {
                case 0:
                    newBall.El.MouseEnter += EllipseColumn0_MouseEnter;
                    newBall.El.MouseLeave += EllipseColumn0_MouseLeave;
                    break;
                case 1:
                    newBall.El.MouseEnter += EllipseColumn1_MouseEnter;
                    newBall.El.MouseLeave += EllipseColumn1_MouseLeave;
                    break;
                case 2:
                    newBall.El.MouseEnter += EllipseColumn2_MouseEnter;
                    newBall.El.MouseLeave += EllipseColumn2_MouseLeave;
                    break;
                case 3:
                    newBall.El.MouseEnter += EllipseColumn3_MouseEnter;
                    newBall.El.MouseLeave += EllipseColumn3_MouseLeave;
                    break;
                case 4:
                    newBall.El.MouseEnter += EllipseColumn4_MouseEnter;
                    newBall.El.MouseLeave += EllipseColumn4_MouseLeave;
                    break;
                case 5:
                    newBall.El.MouseEnter += EllipseColumn5_MouseEnter;
                    newBall.El.MouseLeave += EllipseColumn5_MouseLeave;
                    break;
                case 6:
                    newBall.El.MouseEnter += EllipseColumn6_MouseLeave;
                    newBall.El.MouseLeave += EllipseColumn6_MouseLeave;
                    break;
            }
        }

        /// <summary>
        /// this is a father function of MoveBall func.
        /// send the ball as thead to the pool thead.
        /// </summary>
        /// <param name="ball"></param>
        private void SmoothBallOnTheBoard(Ball ball)
        {
            Canvas.SetTop(ball.El, 18); // set drop pos.
            Canvas.SetLeft(ball.El, ball.X); // set left size from 0,0.
            CanvasBlueGameBoard.Children.Add(ball.El); // add to the canvas.
            ThreadPool.QueueUserWorkItem(MoveBall, ball); // run the ball.
        }

        /// <summary>
        /// display winning row disk/ball at diffrent color border.
        /// </summary>
        
        private void ShowWinningRowDisks(List<Point> winningPointsList, bool isIWin)
        {
            foreach (var point in winningPointsList)
            {

                int row = (int) point.X;
                switch (row) 
                {
                    case 0:
                        row = 18;
                        break;
                    case 1:
                        row = 93;
                        break;
                    case 2:
                        row = 168;
                        break;
                    case 3:
                        row = 243;
                        break;
                    case 4:
                        row = 318;
                        break;
                    case 5:
                        row = 396;
                        break;
                }
                int col = (int) point.Y;
                switch (col) 
                {
                    case 0:
                        col = 15;
                        break;
                    case 1:
                        col = 114;
                        break;
                    case 2:
                        col = 206;
                        break;
                    case 3:
                        col = 298;
                        break;
                    case 4:
                        col = 392;
                        break;
                    case 5:
                        col = 488;
                        break;
                    case 6:
                        col = 580;
                        break;
                }

                var el = new Ellipse();
                el.Width = 70;
                el.Height = 70;
                if (isIWin)
                    el.Fill = _myColorBrush;
                else
                    el.Fill = _opponentColorBrush;
               
                el.StrokeThickness = 4;
                el.Stroke = new SolidColorBrush(Colors.DarkGreen);
                Canvas.SetTop(el, row);
                Canvas.SetLeft(el, col);
                CanvasBlueGameBoard.Children.Add(el);
            }
        }

        /// <summary>
        /// this func called when client disconnect for live game, his opponent (this client) will inform.
        /// </summary>
        private void NotifyOpponentDisconnect()
        {
            TbUserOneScore.Text = "Score: 1000";
            TbUserTwoScore.Text = "Score: 0";
            EndGame(": " + _opponentUserName + " Disconnected From The Game!\n" + "You Automatically Win!!!");
        }

        /// <summary>
        ///  this function will called when client want exit the "FourRowGameWindow".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FourRowGameWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_endGame)
            {
                var result = MessageBox.Show(_userName + " Are You Sure You Want To Exit The Game?",
                    "Exit!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (_fourRowGame.IsMyTurn)
                    {
                        if (WaitingRoomWindow.CheckPingToServer(Client))
                        {
                            ExitMiddleLiveGame();
                        }
                        else
                        {
                            MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            ShowWaitingRoomAfterEndGame(_opponentUserName);
        }

        /// <summary>
        /// this function called when client want to exit and lose.
        /// </summary>
        private void ExitMiddleLiveGame()
        {
            try
            {
                if (Client.StillPlayingTogether(_userName, _opponentUserName)) //check if still playing..
                {
                    Client.EndGame(_opponentUserName, 1000, _userName, 0);
                }
                else
                {
                    Client.EndGame(_userName, 1000, _opponentUserName, 0); // update the server that his want to exit and lose.
                    EndGame(": " + _opponentUserName + " Disconnected From The Game Before You!\n" + "You Automatically Win!!!");
                }
            }
            catch (FaultException<OpponentDisconnectedFault> fault)
            {
                MessageBox.Show(fault.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FaultException<DbFault> fault)
            {
                if (fault.InnerException != null)
                    MessageBox.Show(fault.Detail.Details + " ##\n##\n" + fault.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("The Server Has Disconnected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _endGame = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Unable to connect to the remote server"))
                    MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// this function called when client get from server a update what was the opponent move.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="score"></param>
        private void OpponentMoveUpdate(Move move, int score)
        {
            _opponentScore = score; // set opponent score at the user info.
            TbUserTwoScore.Text = "Score: " + score.ToString();

            var ball = new Ball(move.Point) // create opponent ball.
            {
                BallColor = _opponentColorBrush
            };

            _pos = move.Pos;
            RegisterMouseToBall(move.Column, ball);
            SmoothBallOnTheBoard(ball); // shot the opponent ball..

            if (move.WinningRowDisks != null) // check if the opponent move was a win, for display.
                ShowWinningRowDisks(move.WinningRowDisks, false);

            _fourRowGame.SetEmptyCell(move.Row, move.Column); // update my logic board game about his move.
            MyTurn(); // update that is my turn to display.
        }

        /// <summary>
        /// this function is end game logistic 
        /// </summary>
        /// <param name="message"></param>
        private void EndGame(string message)
        {
            _timer.Stop(); // stop timer game.
            _endGame = true; // game has end.
            TbTurn.Text = "The Game Ended, Please Exit!";
            TbTurn.Foreground = new SolidColorBrush(Colors.DarkBlue);
            if(!string.IsNullOrEmpty(message)) // dispaly message that game end with result (lose, tie, win- when opponent leave the game)
                MessageBox.Show(_userName + message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// this function is a logistic to my turn mod.
        /// </summary>
        private void MyTurn()
        {
            _fourRowGame.IsMyTurn = true;
            TbTurn.Text = "Your Turn!";
            TbTurn.Foreground = new SolidColorBrush(Colors.DarkGreen);
            CurrentPlayer.Fill = _myColorBrush;
        }

        /// <summary>
        /// this function is a logistic to my opponent turn mod.
        /// </summary>
        private void NotMyTurn()
        {
            _fourRowGame.IsMyTurn = false;
            TbTurn.Text = "Not Your Turn!";
            TbTurn.Foreground = new SolidColorBrush(Colors.DarkRed);
            CurrentPlayer.Fill = _opponentColorBrush;
        }

        /// <summary>
        /// this function is the starter of game timer.
        /// </summary>
        public void StartTimerTick()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.1);
            _timer.Tick += delegate { TimerTickReply(); };
            _timer.Start();
        }

        /// <summary>
        /// this function called every second and update the timer string.
        /// </summary>
        private void TimerTickReply()
        {
            const int secondsPerMinute = 60;

            string timerStr;
            _ticks++;
            var seconds = _ticks % secondsPerMinute;
            var minutes = _ticks / secondsPerMinute;
            if (minutes < 10)
                timerStr = "0" + minutes;
            else 
                timerStr = minutes.ToString();

            if (seconds < 10)
                timerStr += ":0" + seconds;
            else
                timerStr += ":" + seconds;

            TbGameTimer.Text = timerStr;
        }


        #region arrow, mouse enter/leave cursor

        private void EllipseColumn0_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack0.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn0_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack0.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        }

        private void EllipseColumn1_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack1.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn1_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack1.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        }

        private void EllipseColumn2_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack2.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn2_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack2.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        }

        private void EllipseColumn3_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack3.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn3_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack3.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        }

        private void EllipseColumn4_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack4.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn4_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack4.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        }

        private void EllipseColumn5_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack5.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn5_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack5.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        }

        private void EllipseColumn6_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageTriBlack6.Visibility = Visibility.Visible;
            Cursor = Cursors.Hand;
        }

        private void EllipseColumn6_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageTriBlack6.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
        } 
        #endregion
    }
}
