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
        public Color Color { get; }
        public Neuron()
        {
            
        }

        public static byte Generate()
        {//гсч
            RNGCryptoServiceProvider c = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[8];
            c.GetBytes(randomNumber);
            return randomNumber[0];
        }

        public Neuron(double[] sample)
        {
            Weights = new double[sample.Length];
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = sample[i];

            Random rand = new Random((int)DateTime.Now.Ticks);

            Color = new Color
            {            
                A = 255,
                R = Generate(),
                G = Generate(),
                B = Generate()
            };
        }

        public Neuron(int countInputs)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            Weights = new double[countInputs];
            for (int i = 0; i < countInputs; i++)
                Weights[i] = rand.Next();
        }
    }
}
