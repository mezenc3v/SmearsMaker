using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmearTracer
{
    public class Segment
    {
        public List<Pixel> Data { get; set; }
        public Pixel CentroidPixel { get; set; }
        public double[] Color { get; set; }
        public Segment()
        {
            Data = new List<Pixel>();
            Color = Generate();
        }
        public bool SuitableTo(Pixel data)
        {
            foreach (var pixel in Data)
            {
                if (Math.Abs(data.X - pixel.X) <= 1 && Math.Abs(data.Y - pixel.Y) <= 1)
                {
                    return true;
                }
            }
            return false;
        }
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
    }
}
