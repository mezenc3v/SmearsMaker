using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using SmearTracer.Core.Abstract;

namespace SmearTracer.Core.Models
{
    public class BrushStroke:SequenceOfParts
    {
        public override double GetLength()
        {
            var length = 0d;
            for (int i = 1; i < Points.Count; i++)
            {
                length += Distance(Points[i - 1].Position, Points[i].Position);
            }

            return length;
        }

        public BrushStroke()
        {
            Points = new List<IUnit>();
        }

        private static double Distance(Point first, Point second)
        {
            var sum = Math.Pow(first.X - second.X, 2);
            sum += Math.Pow(first.Y - second.Y, 2);
            return Math.Sqrt(sum);
        }

        private static double[] Generate()
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
