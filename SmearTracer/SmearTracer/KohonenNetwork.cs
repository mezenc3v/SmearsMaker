using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmearTracer
{
    public class KohonenNetwork
    {
        public NeuronNetwork Network { get; set; }

        public KohonenNetwork()
        {
            
        }
        public KohonenNetwork(List<Neuron> net)
        {
            Network = new NeuronNetwork {NeuronsList = net};
        }

        private List<Pixel> Winner(List<Pixel> data, int winnerNeuronPoint)
        {
            List<Pixel> winnerData = new List<Pixel>();
            Parallel.ForEach(data, pixel =>
            {
                if (winnerNeuronPoint == WinnerNeuron(pixel.Coordinates))
                {
                    lock (data)
                    {
                        winnerData.Add(pixel);
                    }
                }
            });

            return winnerData;
        }

        public void Learning(List<Pixel> data, int studIteration, int length)
        {
            //creating network
            Network = new NeuronNetwork(length, data);
            //learning network
            for (int j = 0; j < studIteration; j++)
                foreach (Pixel pixel in data)
                {
                    LearningNetwork(pixel.Coordinates);
                }
            //applyng data changes
            for (int i = 0; i < Network.NeuronsList.Count; i++)
            {
                Network.NeuronsList[i].Data = Winner(data, i);

                if (Network.NeuronsList[i].Data.Count > 0)
                {
                    double[] averageData = new double[data[0].Data.Length];

                    foreach (Pixel pixel in Network.NeuronsList[i].Data)
                    {
                        for (int l = 0; l < averageData.Length; l++)
                        {
                            averageData[l] += pixel.Data[l];
                        }
                    }
                    for (int l = 0; l < averageData.Length; l++)
                    {
                        averageData[l] /= Network.NeuronsList[i].Data.Count;
                    }
                    Network.NeuronsList[i].AverageData = averageData;
                }
            }
        }

        private void LearningNetwork(double[] input)
        {
            int winner = WinnerNeuron(input);
            for (int x = 0; x < Network.NeuronsList.Count; x++)
            {
                if (Math.Abs(winner - x) == 0)
                {
                    double k = 1;
                    double gauss = GaussFunction(k);

                    for (int j = 0; j < input.Length; j++)
                    {
                        k = input[j] - Network.NeuronsList[x].Weights[j];
                        Network.NeuronsList[x].Weights[j] += gauss * k;
                    }
                }
            }
        }

        private double Distance(Neuron neuron, double[] input)
        {
            double sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                sum += Math.Abs(input[i] - neuron.Weights[i]);
                //sum += Math.Pow(input[i] - neuron.Weights[i], 2);
            }

            return sum;
        }

        private int WinnerNeuron(double[] input)
        {
            int x = 0;

            if (Network.NeuronsList.Count > 0)
            {
                double minDistance = Distance(Network.NeuronsList[0], input);
                for (int i = 0; i < Network.NeuronsList.Count; i++)
                {
                    double distance = Distance(Network.NeuronsList[i], input);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        x = i;
                    }
                }
            }
            return x;
        }

        private double GaussFunction(double k)
        {
            double sigma;
            //double sigma = -0.01 * k+ 2;
            //double sigma = Math.Exp(-k/(1000/Math.Log(270)));
            //double sigma = Math.Exp(-k / 5);
            //double sigma = 5 * Math.Sqrt(Network.NeuronsList.Count * Network.NeuronsList[0].Length) / Math.Sqrt(k);
            //sigma = Math.Exp(-k * k / (2 * sigma * sigma));

            //if (k != 0)
            //{
            sigma = 1 / (k * 1000 + 7000);
            //}
            //else
            //{
            //sigma = 1;
            //}
            return sigma;
        }
    }
}
