using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that represent all details of user that register to the game.
    /// </summary>
    public class Result
    {
        public string UserName { set; get; }

        public int NumberOfGames { set; get; }

        public int NumberOfVictory { set; get; }

        public int NumberOfLosses { set; get; }

        public int NumberOfPoints { set; get; }

    }
}
