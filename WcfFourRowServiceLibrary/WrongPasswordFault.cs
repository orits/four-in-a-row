using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that using the throw exception when client that try to connect has the wrong password as saved at the DB table.
    /// </summary>
    [DataContract]
    public class WrongPasswordFault
    {
        [DataMember]
        public string Details { get; set; }
    }
}
