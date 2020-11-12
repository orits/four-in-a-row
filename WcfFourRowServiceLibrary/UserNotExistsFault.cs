using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that using the throw exception when client not exists at the DB (like connect).
    /// </summary>
    [DataContract]
    public class UserNotExistsFault
    {
        [DataMember]
        public string Details { get; set; }
    }
}
