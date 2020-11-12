using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace WcfFourRowServiceLibrary
{
    /// <summary>
    /// class that represent a Move from the game, with details about the move.
    /// </summary>
    public class Move
    {
        public int Pos { set; get; }

        public Point Point { set; get; }

        public int Row { set; get; }

        public int Column { set; get; }

        public List<Point> WinningRowDisks { set; get; } // this is only when winning has be done.
    }
}
