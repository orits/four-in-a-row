using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that using the throw exception when the opponent disconnect form the game.
    /// </summary>
    [DataContract]
    internal class OpponentDisconnectedFault
    {
        [DataMember]
        public string Details { get; set; }
    }
}
