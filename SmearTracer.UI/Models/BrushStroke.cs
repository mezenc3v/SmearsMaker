using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmearTracer.UI.Models
{
    public class BrushStroke
    {
        public double[] Color;
        public List<Point> Points;
        public int Width;
        public double Length => Points.Select((t, i) => Distance(t, Points[i + 1])).Sum();

        public BrushStroke()
        {
            Points = new List<Point>();
            Color = Generate();
        }

        private static double Distance(Point first, Point second)
        {
            var sum = Math.Pow(first.X - second.X, 2);
            sum += Math.Pow(first.Y - second.Y, 2);
            return Math.Sqrt(sum);
        }

        public double[] Generate()
        {
            var c = new RNGCryptoServiceProvider();
            var randomNumber = new byte[3];
            c.GetBytes(randomNumber);

            var r = randomNumber[0];
            var g = randomNumber[1];
            var b = randomNumber[2];

            double[] color = { r, g, b, 255 };

            return color;
        }
    }
}
