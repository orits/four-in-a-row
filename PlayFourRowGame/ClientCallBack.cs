using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using PlayFourRowGame.FourRowServiceReference;

namespace PlayFourRowGame
{
    /// <summary>
    /// implements class of IFourRowCallback interface that represent the "Client" operations that the Server" can ask.
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientCallBack : IFourRowServiceCallback
    {
        #region Delegates

        internal Action OpponentDisconnect; 

        internal Action<string> EndGame;

        internal Action<string> InviteToPlay;

        internal Action<string, bool> ReplyInviteToPlay;

        internal Action<Move, int> UpdateGame;



        #endregion

        /// <summary>
        /// that function is called when clientDisconnect form a live game, the server will inform the client.
        /// </summary>
        public void InformOpponentDisconnect()
        {
            OpponentDisconnect();
        }

        /// <summary>
        /// that function is called when client invited other client, server will ask the client.
        /// </summary>
        public void DidYouWantToPlay(string userName)
        {
            InviteToPlay(userName);
        }

        /// <summary>
        /// that function is called when invited client reply to this client invite, server will update him of the state of his invite.
        /// </summary>
        public void ReplyDidYouWantToPlay(string userName, bool reply)
        {
            ReplyInviteToPlay(userName, reply);
        }

        /// <summary>
        /// that function is called when client that playing a live game get for the server update of opponent move & score.
        /// </summary>
        public void OtherPlayerReportMove(Move move, MoveResult otherPlayerMoveResult, int score)
        {
            UpdateGame(move, score); // will update client board & screen after opponent move.

            if (otherPlayerMoveResult == MoveResult.Draw)
            {
                EndGame(": It's a draw!");
            }
            else if (otherPlayerMoveResult == MoveResult.YouWon)
            {
                EndGame(": You lost...!");
            }

        }
    }
}
