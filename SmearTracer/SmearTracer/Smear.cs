using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;

namespace SmearTracer
{
    public class Smear
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double[] AverageData { get; set; }
        public List<Pixel> Data { get; set; }
        public double[] Color { get; set; }

        private double[] Generate()
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

        public Smear(Pixel sample)
        {
            X = sample.X;
            Y = sample.Y;
            AverageData = sample.Data;
            Data = new List<Pixel>();
            Color = Generate();
        }
    }
}
