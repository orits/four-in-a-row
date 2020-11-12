using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that using the throw DB exception.
    /// </summary>
    [DataContract]
    public class DbFault
    {
        [DataMember]
        public string Details { get; set; }
    }
}
