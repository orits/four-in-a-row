using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfFourRowServiceLibrary
{
    [ServiceContract(CallbackContract = typeof(IFourRowCallback),
     SessionMode = SessionMode.Required)]

    /// <summary>
    /// IFourRowService interface that represent the "Server" operations that the "Client" can ask.
    /// </summary>
    public interface IFourRowService
    {
        /// <summary>
        /// that function is register the new client to the DB.
        /// </summary>
        [FaultContract(typeof(UserExistsFault))]
        [FaultContract(typeof(DbFault))]
        [OperationContract(IsOneWay = false)]
        bool Register(string userName, string hashedPassword, string emojiIcon);

        /// <summary>
        /// that function is connect the registered client to the Server.
        /// </summary>
        [FaultContract(typeof(UserExistsFault))]
        [FaultContract(typeof(UserNotExistsFault))]
        [FaultContract(typeof(WrongPasswordFault))]
        [OperationContract(IsInitiating = true)]
        void Connect(string userName, string hashedPassword);

        /// <summary>
        /// that function is disconnect the client from the Server.
        /// </summary>
        [OperationContract]
        void Disconnect(string userName);

        /// <summary>
        /// that function is update the DB, about new live game and update clients as playing.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        void StartGame(string fromUserName, string toUserName);

        /// <summary>
        /// that function is update the DB, about live game that end. and update clients as no playing.
        /// also update the DB about history game. 
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        void EndGame(string winnerUserName, int winnerScore, string loseUserName, int loseScore);

        /// <summary>
        /// that function is update clients as waiting.
        /// this called after game end and returning to the waiting room.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        void BackToWaitingClients(string userName, string opponentUserName);

        /// <summary>
        /// that function is happened after every step at the game, also send the last score of the user.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        [FaultContract(typeof(ExceptionFault))]
        void ReportMove(string fromUserName, Move move, string toUserName, MoveResult moveResult, int score);

        /// <summary>
        /// that function is call after client invite other client to play.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        void InviteOtherPlayer(string fromUserName, string toUserName);

        /// <summary>
        /// that function is call after client that invited reply to the invite.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        void ReplyToInviteOtherPlayer(string fromUserName, string toUserName, bool reply);

        /// <summary>
        /// that function is check if pear of client playing.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OpponentDisconnectedFault))]
        bool StillPlayingTogether(string userName, string opponentUserName);

        /// <summary>
        /// that function is call to update waiting client that can play.
        /// </summary>
        [OperationContract]
        IEnumerable<string> GetClients();

        /// <summary>
        /// that function is call before a new game to get access to clients images form the DB.
        /// </summary>
        [OperationContract]
        List<string> GetImageClients(string userNameOne, string userNameTwo);

        /// <summary>
        /// that function is call the check ping the server.
        /// </summary>
        [OperationContract]
        bool Ping();

        /// <summary>
        /// that function sort all register users.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        List<Result> SortAllRegisterUsers(string fromUserName, string by);

        /// <summary>
        /// that function is return all live games that playing now.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        List<LiveGame> ShowAllLiveGames();

        /// <summary>
        /// that function is return all history games that played.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        List<HistoryGame> ShowAllHistoryGames();

        /// <summary>
        /// that function is return all register clients of the game in the DB.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        List<string> GetAllRegisterClients();

        /// <summary>
        /// that function is return all history games that played with the 2 clients.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        List<HistoryGame> ShowAllHistoryGamesTwoPlayers(List<string> playersList);

        /// <summary>
        /// that function is return all date about selected client.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DbFault))]
        [FaultContract(typeof(ExceptionFault))]
        Tuple<List<HistoryGame>, Result> ShowAllPlayerHistoryGames(string userName);
    }


    /// <summary>
    /// IFourRowCallback interface that represent the "Client" operations that the Server" can ask.
    /// </summary>
    public interface IFourRowCallback
    {

        /// <summary>
        /// that function is called when clientDisconnect form a live game, the server will inform the client.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void InformOpponentDisconnect();

        /// <summary>
        /// that function is called when client invited other client, server will ask the client.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void DidYouWantToPlay(string userName);

        /// <summary>
        /// that function is called when invited client reply to this client invite, server will update him of the state of his invite.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ReplyDidYouWantToPlay(string userName, bool reply);

        /// <summary>
        /// that function is called when client that playing a live game get for the server update of opponent move & score.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OtherPlayerReportMove(Move move, MoveResult otherPlayerMoveResult, int score);
    }
}
