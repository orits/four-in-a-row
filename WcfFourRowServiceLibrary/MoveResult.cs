using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that represent a enum that represent a move result at the game.
    /// </summary>
    [DataContract]
    public enum MoveResult
    {
        [EnumMember]
        YouWon,
        [EnumMember]
        Draw,
        [EnumMember]
        NotYourTurn,
        [EnumMember]
        GameOn
    }
}
