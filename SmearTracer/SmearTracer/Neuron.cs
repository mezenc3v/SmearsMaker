using System.Collections.Generic;
using System.Security.Cryptography;

namespace SmearTracer
{
    public class Neuron
    {
        public double[] Weights { get; set; }
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

        public Neuron(double[] sample)
        {
            Weights = new double[sample.Length];
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = sample[i];

            Data = new List<Pixel>();
            Color = Generate();
        }
    }
}
