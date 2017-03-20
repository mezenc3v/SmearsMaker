using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmearTracer
{
    public class Neuron
    {
        public double[] Weights { get; set; }
        public double[] AverageData { get; set; }
        public List<Pixel> Data { get; set; }
        public double[] Color { get; set; }
        public Neuron()
        {
            
        }

        public static byte Generate()
        {//гсч
            RNGCryptoServiceProvider c = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[1];
            c.GetBytes(randomNumber);

            return randomNumber[0];
        }

        public Neuron(double[] sample)
        {
            Weights = new double[sample.Length];
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = sample[i];

            Data = new List<Pixel>();
            byte r = Generate();
            byte g = Generate();
            byte b = Generate();
            Color = new double[]{255, r, g, b};
        }

        public Neuron(int countInputs)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            Weights = new double[countInputs];
            for (int i = 0; i < countInputs; i++)
                Weights[i] = rand.Next(200);

            Color = new double[] { 255, Generate(), Generate(), Generate() };
        }
    }
}
