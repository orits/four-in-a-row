using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that using the throw exception when client exists at the DB (like connect or register).
    /// </summary>
    [DataContract]
    public class UserExistsFault
    {
        [DataMember]
        public string Details { get; set; }
    }
}
