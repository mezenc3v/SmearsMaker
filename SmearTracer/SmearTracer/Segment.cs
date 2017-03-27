using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;

namespace SmearTracer
{
    public class Segment
    {
        public List<Pixel> Data { get; set; }
        public Pixel CentroidPixel { get; set; }
        public Point MinXPoint { get; set; }
        public Point MinYPoint { get; set; }
        public Point MaxXPoint { get; set; }
        public Point MaxYPoint { get; set; }
        public double[] Color { get; set; }

        public Segment()
        {
            Data = new List<Pixel>();
            Color = Generate();
            MinXPoint = new Point();
            MaxXPoint = new Point();
            MinYPoint = new Point();
            MaxYPoint = new Point();
        }

        public bool CompareTo(Pixel data)
        {
            for (int i = Data.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(data.X - Data[i].X) < 2 && Math.Abs(data.Y - Data[i].Y) < 2)
                {
                    return true;
                }
            }
            return false;
        }

        private static double[] Generate()
        {
            RNGCryptoServiceProvider c = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[3];
            c.GetBytes(randomNumber);

            byte r = randomNumber[0];
            byte g = randomNumber[1];
            byte b = randomNumber[2];

            double[] color = { r, g, b, 255 };

            return color;
        }
    }
}
