using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using FourRowGame;

/// <summary>
/// implements class of IFourRowService interface.
/// </summary>

namespace WcfFourRowServiceLibrary
{
    // singleton and multi clients, for debug mod, for more info exception when happened.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false,
        IncludeExceptionDetailInFaults = true)]
    public class FourRowService : IFourRowService
    {
        Dictionary<string, IFourRowCallback> _clients = new Dictionary<string, IFourRowCallback>(); // connected users (live)
        Dictionary<string,bool> _liveClients = new Dictionary<string, bool>();    // connected users (live) that can play.
        Dictionary<string, bool> _invitedClients = new Dictionary<string,bool>(); // connected users (live) that can has invited to play.
        List<Tuple<string, string>> _playingClients = new List<Tuple<string, string>>(); // connected users that playing.


        public IFourRowCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IFourRowCallback>();
            }
        }

        object syncObj = new object(); 

        /// <summary>
        /// that function is register the new client to the DB.
        /// </summary>
        public bool Register(string userName, string hashedPassword, string emojiIcon)
        {
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {

                    var user = (from u in ctx.Users
                                where u.UserName == userName
                                select u).FirstOrDefault();

                    if (user == null)
                    {
                        ctx.Users.Add(new User
                        {
                            UserName = userName,
                            HashedPassword = hashedPassword,
                            EmojiIcon = emojiIcon
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        var userExists = new UserExistsFault
                        {
                            Details = "User name: " + userName + " already Exits On DB, Try something else!"
                        };
                        throw new FaultException<UserExistsFault>(userExists);
                    }
                    return true;
                }
            }
            catch(FaultException<UserExistsFault> fault)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is connect the registered client to the Server.
        /// </summary>
        public void Connect(string userName, string hashedPassword)
        {
            lock (syncObj)
            {
                if (_clients.ContainsKey(userName))
                {
                    var userExists = new UserExistsFault
                    {
                        Details = "User name: " + userName + " already Connected, Try something else!"
                    };
                    throw new FaultException<UserExistsFault>(userExists);
                }

                using (var ctx = new FourInRowDB_Context())
                {
                    var user = (from u in ctx.Users
                        where u.UserName == userName
                        select u).FirstOrDefault();

                    if (user == null)
                    {
                        var userNotExists = new UserNotExistsFault
                        {
                            Details = "User name: " + userName + " Not Exits On DB, Please Register First!"
                        };
                        throw new FaultException<UserNotExistsFault>(userNotExists);
                    }
                    else if (user.HashedPassword != hashedPassword)
                    {
                        var fault = new WrongPasswordFault
                        {
                            Details = "Wrong Password, Please Try Again!"
                        };
                        throw new FaultException<WrongPasswordFault>(fault);
                    }
                    else
                    {
                        _clients.Add(userName, OperationContext.Current.GetCallbackChannel<IFourRowCallback>());
                        _liveClients.Add(userName, true);
                    }
                }
            }
        }

        /// <summary>
        /// that function is disconnect the client from the Server.
        /// </summary>
        public void Disconnect(string userName)
        {
            _clients.Remove(userName);
            _liveClients.Remove(userName);
            _invitedClients.Remove(userName);
        }

        /// <summary>
        /// that function is update the DB, about new live game and update clients as playing.
        /// </summary>
        public void StartGame(string fromUserName, string toUserName)
        {
            try
            {
                var liveGame = new LiveGame
                {
                    UserNameOne = fromUserName,
                    UserNameTwo = toUserName,
                    StartingDateTime = DateTime.Now
                };
                _playingClients.Add(Tuple.Create(toUserName, fromUserName));

                using (var ctx = new FourInRowDB_Context())
                {
                    ctx.LiveGames.Add(liveGame);
                    
                    ctx.SaveChanges();
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is update the DB, about live game that end. and update clients as no playing.
        /// also update the DB about history game. 
        /// </summary>
        public void EndGame(string winnerUserName, int winnerScore, string loseUserName, int loseScore)
        {
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    var liveGame = ctx.LiveGames.FirstOrDefault(g => (g.UserNameOne == winnerUserName || g.UserNameOne == loseUserName)
                                                                     && (g.UserNameTwo == winnerUserName || g.UserNameTwo == loseUserName));

                    var isUserOneWin = liveGame != null && winnerUserName == liveGame.UserNameOne;
                    var isWinGame = winnerScore > loseScore;

                    var historyGame = new HistoryGame
                    {
                        GameId = liveGame.GameId,
                        UserNameOne = liveGame.UserNameOne,
                        UserNameTwo = liveGame.UserNameTwo,
                        StartingDateTime = liveGame.StartingDateTime,
                        WinUserName = (isWinGame) ? winnerUserName : "* Draw *",
                        LossUserName = (isWinGame) ? loseUserName : "* Draw *",
                        UserNameOneScore = (isUserOneWin) ? winnerScore : loseScore,
                        UserNameTwoScore = (isUserOneWin) ? loseScore : winnerScore,
                        EndingDateTime = DateTime.Now
                    };

                    ctx.HistoryGames.Add(historyGame);

                    ctx.LiveGames.Remove(liveGame);

                    ctx.SaveChanges();
                }

                _playingClients.Remove(Tuple.Create(winnerUserName, loseUserName));
                _playingClients.Remove(Tuple.Create(loseUserName, winnerUserName));
            }
            catch (DbUpdateException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is update clients as waiting.
        /// this called after game end and returning to the waiting room.
        /// </summary>
        public void BackToWaitingClients(string userName, string opponentUserName)
        {
            _playingClients.Remove(Tuple.Create(userName, opponentUserName));
            _playingClients.Remove(Tuple.Create(opponentUserName, userName));
            _invitedClients[userName] = false;
        }

        /// <summary>
        /// that function is happened after every step at the game, also send the last score of the user.
        /// </summary>
        public void ReportMove(string fromUserName, Move move, string toUserName, MoveResult moveResult, int score)
        {
            try
            {
                if (_clients.ContainsKey(toUserName) && 
                    (_playingClients.Contains(Tuple.Create(fromUserName, toUserName)) || _playingClients.Contains(Tuple.Create(toUserName, fromUserName))))
                {
                    Thread updateOtherPlayerMoveThread = new Thread(() =>
                        {
                            _clients[toUserName].OtherPlayerReportMove(move, moveResult, score);
                        }
                    );
                    updateOtherPlayerMoveThread.Start();
                }
                else
                {
                    EndGame(fromUserName, 1000, toUserName, 0);
                    var opponentDisconnected = new OpponentDisconnectedFault
                    {
                        Details = ": " + toUserName + " Disconnected From The Game!\n" + "You Automatically Win!!!"
                    };
                    throw new FaultException<OpponentDisconnectedFault>(opponentDisconnected);
                }
            }
            catch (FaultException<OpponentDisconnectedFault> fault)
            {
                throw fault;
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is call after client invite other client to play.
        /// </summary>
        public void InviteOtherPlayer(string fromUserName, string toUserName)
        {
            if (_liveClients.ContainsKey(toUserName) && _liveClients[toUserName])
            {
                _invitedClients[toUserName] = true;
                _invitedClients[fromUserName] = true;

                var thread = new Thread(() =>
                {
                    _clients[toUserName].DidYouWantToPlay(fromUserName);
                });

                thread.Start();
            }
            else
            {
                var opponentDisconnected = new OpponentDisconnectedFault
                {
                    Details = "User name: " + toUserName + " Disconnected From The Waiting Room, Try Other One!"
                };
                throw new FaultException<OpponentDisconnectedFault>(opponentDisconnected);
            }
        }

        /// <summary>
        /// that function is call after client that invited reply to the invite.
        /// </summary>
        public void ReplyToInviteOtherPlayer(string fromUserName, string toUserName, bool reply)
        {
            if (_liveClients.ContainsKey(toUserName) && _liveClients[toUserName])
            {
                if (!reply)
                {
                    _invitedClients[toUserName] = false;
                    _invitedClients[fromUserName] = false;
                }
                var thread = new Thread(() =>
                {
                    _clients[toUserName].ReplyDidYouWantToPlay(fromUserName, reply);
                });

                thread.Start();
            }
            else
            {
                var opponentDisconnected = new OpponentDisconnectedFault
                {
                    Details = "User name: " + toUserName + " Disconnected From The Waiting Room, Wait For Other Invite Or Invite User."
                };
                throw new FaultException<OpponentDisconnectedFault>(opponentDisconnected);
            }
        }

        /// <summary>
        /// that function is check if pear of client playing.
        /// </summary>
        public bool StillPlayingTogether(string userName, string opponentUserName)
        {
            if (_liveClients.ContainsKey(opponentUserName) && _liveClients[opponentUserName])
            {
                var a = _playingClients.Contains(Tuple.Create(userName, opponentUserName));
                var b = _playingClients.Contains(Tuple.Create(opponentUserName, userName));

                if (a || b)
                {
                    var thread = new Thread(() =>
                    {
                        _clients[opponentUserName].InformOpponentDisconnect();
                    });
                    thread.Start();
                }

                return a || b;
            }

            var opponentDisconnected = new OpponentDisconnectedFault
            {
                Details = "User name: " + opponentUserName + " Disconnected From The Waiting Room!"
            };
            throw new FaultException<OpponentDisconnectedFault>(opponentDisconnected);
        }

        /// <summary>
        /// that function is call to update waiting client that can play.
        /// </summary>
        public IEnumerable<string> GetClients()
        {
            lock (syncObj)
            {
                var resultOne =
                    _liveClients.Where(k => k.Value).Select(k => k.Key); // all live clients that didn't play.
                var resultTwo =
                    _invitedClients.Where(k => k.Value).Select(k => k.Key); // all live clients that invited.
                return resultOne.Except(resultTwo); // resultOne - resultTwo.
            }
        }

        /// <summary>
        /// that function is call before a new game to get access to clients images form the DB.
        /// </summary>
        public List<string> GetImageClients(string userNameOne, string userNameTwo)
        {
            try
            {
                var result = new List<string>();
                using (var ctx = new FourInRowDB_Context())
                {
                   var emojiOne = ctx.Users.Where(u => u.UserName == userNameOne)
                        .Select(u => u.EmojiIcon).FirstOrDefault();
                   var emojiTwo = ctx.Users.Where(u => u.UserName == userNameTwo)
                       .Select(u => u.EmojiIcon).FirstOrDefault();
                   result.Add(emojiOne);
                   result.Add(emojiTwo);

                   return result;
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is call the check ping the server.
        /// </summary>
        public bool Ping()
        {
            return true;
        }

        /// <summary>
        /// that function sort all register users.
        /// </summary>
        public List<Result> SortAllRegisterUsers(string fromUserName, string by)
        {
            var list = new List<Result>();
           
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    var q = (ctx.Users.Select(u => u.UserName)).ToList();
                    foreach (var userName in q)
                    {
                        list.Add(
                            new Result
                            {
                                UserName = userName,
                                NumberOfGames = FindNumberOfGames(userName),
                                NumberOfVictory = FindNumberOfVictory(userName),
                                NumberOfLosses = FindNumberOfLosses(userName),
                                NumberOfPoints = FindNumberOfPoints(userName)
                            });
                    }
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }


            switch (by)
            {
                case "UserName":
                   return list.OrderBy(r => r.UserName).ToList();
                case "NumberOfGames":
                    return list.OrderBy(r => r.NumberOfGames).ToList();
                case "NumberOfVictory":
                    return list.OrderBy(r => r.NumberOfVictory).ToList();
                case "NumberOfLosses":
                    return list.OrderBy(r => r.NumberOfLosses).ToList();
                case "NumberOfPoints":
                    return list.OrderBy(r => r.NumberOfPoints).ToList();
                default:
                    return null;
            }
        }

        /// <summary>
        /// that function is return all live games that playing now.
        /// </summary>
        public List<LiveGame> ShowAllLiveGames()
        {
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    return ctx.LiveGames.Select(g => g).ToList();
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is return all history games that played.
        /// </summary>
        public List<HistoryGame> ShowAllHistoryGames()
        {
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    return ctx.HistoryGames.Select(g => g).ToList();
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is return all register clients of the game in the DB.
        /// </summary>
        public List<string> GetAllRegisterClients()
        {
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    return ctx.Users.Select(user => user.UserName).ToList();
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is return all history games that played with the 2 clients.
        /// </summary>
        public List<HistoryGame> ShowAllHistoryGamesTwoPlayers(List<string> playersList)
        {
            var userNameOne = playersList[0];
            var userNameTwo = playersList[1];
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    var player_one = (ctx.HistoryGames
                        .Where(h => (h.UserNameOne == userNameOne || h.UserNameTwo == userNameOne))).ToList();

                    var player_two = (ctx.HistoryGames
                        .Where(h => (h.UserNameOne == userNameTwo || h.UserNameTwo == userNameTwo))).ToList();

                    var intersectHistoryGames = player_one.Intersect(player_two).ToList();
                    if (intersectHistoryGames.Count > 0)
                        return intersectHistoryGames;

                    return null;
                }
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is return all date about selected client.
        /// </summary>
        public Tuple<List<HistoryGame>, Result> ShowAllPlayerHistoryGames(string userName)
        {
            var list = new List<HistoryGame>();
            try
            {
                using (var ctx = new FourInRowDB_Context())
                {
                    list = (ctx.HistoryGames
                            .Where(h => (h.UserNameOne == userName || h.UserNameTwo == userName)))
                            .ToList();
                    
                }
                var result = new Result
                {
                    UserName = userName,
                    NumberOfGames = FindNumberOfGames(userName),
                    NumberOfVictory = FindNumberOfVictory(userName),
                    NumberOfLosses = FindNumberOfLosses(userName),
                    NumberOfPoints = FindNumberOfPoints(userName)
                };

                return Tuple.Create(list, result);
            }
            catch (DbException ex)
            {
                var dFault = new DbFault()
                {
                    Details = ex.Message
                };
                throw new FaultException<DbFault>(dFault);
            }
            catch (Exception ex)
            {
                var exceptionFault = new ExceptionFault()
                {
                    Details = ex.Message + "\n" + "Type: " + ex.GetType()
                };
                throw new FaultException<ExceptionFault>(exceptionFault);
            }
        }

        /// <summary>
        /// that function is finding the count of game of selected client.
        /// </summary>
        private int FindNumberOfGames(string userName)
        {
            using (var ctx = new FourInRowDB_Context())
            {
                return ctx.HistoryGames.Where(g => g.UserNameOne == userName || g.UserNameTwo == userName).ToList().Count;
            }
        }

        /// <summary>
        /// that function is finding the count of winning games of selected client.
        /// </summary>
        private int FindNumberOfVictory(string userName)
        {
            using (var ctx = new FourInRowDB_Context())
            {
                return ctx.HistoryGames.Where(g => g.WinUserName == userName).ToList().Count;
            }
        }

        /// <summary>
        /// that function is finding the count of losing games of selected client.
        /// </summary>
        private int FindNumberOfLosses(string userName)
        {
            using (var ctx = new FourInRowDB_Context())
            {
                return ctx.HistoryGames.Where(g => g.LossUserName == userName).ToList().Count;
            }
        }

        /// <summary>
        /// that function is finding the number of score of all games of a selected client.
        /// </summary>
        private int FindNumberOfPoints(string userName)
        {
            using (var ctx = new FourInRowDB_Context())
            {
                int fq1 = 0, fq2 = 0;
                var q1 = ctx.HistoryGames.Where(g => g.UserNameOne == userName).ToList();
                var q2 = ctx.HistoryGames.Where(g => g.UserNameTwo == userName).ToList();
                if(q1.Count > 0)
                    fq1 = (int) q1.Sum(g => g.UserNameOneScore);
                if(q2.Count > 0)
                    fq2 = (int) q2.Sum(g => g.UserNameTwoScore);
                return fq1 + fq2;
            }
        }
    }
}
