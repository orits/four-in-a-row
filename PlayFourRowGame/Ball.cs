using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PlayFourRowGame
{
    /// <summary>
    /// that class represent the ball\disk of the four row game board. (gui view)
    /// </summary>
    public class Ball
    {
        private const int EllipseWidth = 70;
        private const int EllipseHeight = 70;
        private const int EllipseSpeed = 20;

        public Ellipse El { get; set; }

        public SolidColorBrush BallColor {

            get { return BallColor; }
            set { El.Fill = value; }
            
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double XSpeed { get; set; }

        public double YSpeed { get; set; }


        public Ball(Point p)
        {
            El = new Ellipse();
            El.Width = EllipseWidth; 
            El.Height = EllipseHeight;
            XSpeed = EllipseSpeed;
            YSpeed = EllipseSpeed;
            X = p.X - El.Width / 2;
            Y = p.Y - El.Height / 2;
        }
    }
}
