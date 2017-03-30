using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;

namespace SmearTracer
{
    public class SuperPixel
    {
        public Point Centroid { get; set; }
        public double[] Color { get; set; }
        public List<Pixel> Data { get; set; }

        public SuperPixel(double x, double y)
        {
            Centroid = new Point(x, y);
            Data = new List<Pixel>();
            Color = Generate();
        }
        public SuperPixel(Point point)
        {
            Centroid = point;
            Data = new List<Pixel>();
            Color = Generate();
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
