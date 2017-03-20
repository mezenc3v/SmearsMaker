using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmearTracer
{
    public class KohonenNetwork
    {
        public NeuronNetwork Network { get; set; }
        private readonly int _learningRadius;
        private readonly int _width;
        private readonly int _height;

        public KohonenNetwork(int width, int height, int radius)
        {
            _width = width;
            _height = height;
            _learningRadius = radius;
        }

        public double[][] PaintDataImage(List<Pixel> segments, double[][] data, int heightImage)
        {
            double[][] paintedDataimage = data;
            for (int i = 0; i < Network.NeuronsMap.Count; i++)
            {
                for (int j = 0; j < Network.NeuronsMap[i].Length; j++)
                {
                    Network.NeuronsMap[i][j].Data = Winner(segments, i, j);
                    if (Network.NeuronsMap[i][j].Data.Count > 0)
                    {
                        double[] averageData = new double[segments[0].Data.Length];
                        foreach (Pixel pixel in Network.NeuronsMap[i][j].Data)
                        {
                            int index = (int)(pixel.Coordinates[0] * heightImage + pixel.Coordinates[1]);
                            pixel.Data = data[index];
                            for (int l = 0; l < averageData.Length; l++)
                            {
                                averageData[l] += data[index][l];
                            }
                        }
                        for (int l = 0; l < averageData.Length; l++)
                        {
                            averageData[l] /= Network.NeuronsMap[i][j].Data.Count;
                        }
                        Network.NeuronsMap[i][j].AverageData = averageData;
                    }
                }
            }

            foreach (var arrayNeurons in Network.NeuronsMap)
            {
                foreach (Neuron neuron in arrayNeurons)
                {
                    foreach (var dataNeuron in neuron.Data)
                    {
                        int index = (int)(dataNeuron.Coordinates[0] * heightImage + dataNeuron.Coordinates[1]);
                        data[index] = neuron.Color;
                        //data[index] = neuron.AverageData;
                    }
                }
            }

            return paintedDataimage;
        }

        private List<Pixel> Winner(List<Pixel> data, int coordX, int coordY)
        {
            List<Pixel> winnerData = new List<Pixel>();
            Point winnerNeuronPoint = new Point(coordX,coordY);

            foreach (Pixel pixel in data)
            {
                if (winnerNeuronPoint == WinnerNeuron(pixel.Coordinates))
                {
                    winnerData.Add(pixel);
                }
            }
            return winnerData;
        }

        public void Learning(List<Pixel> data,int studIteration)
        {
            //creating network
            Network = new NeuronNetwork(_width, _height, data);
            //learning network
            for (int j = 0; j < studIteration; j++)
                for (int x = 0; x < data.Count; x++)
                {            
                    LearningNetwork(data[x].Coordinates);
                }
        }

        private void LearningNetwork(double[] input)
        {
            Point winner = WinnerNeuron(input);
            for (int x = 0; x < Network.NeuronsMap.Count; x++)
            {
                for (int y = 0; y < Network.NeuronsMap[x].Length; y++)
                {
                    if (Math.Abs(winner.X - x) <= _learningRadius && Math.Abs(winner.Y - y) <= _learningRadius)
                    {
                        double k = Math.Sqrt(Math.Pow(winner.X - x, 2) + Math.Pow(winner.Y - y, 2));
                        double gauss = GaussFunction(k);

                        for (int j = 0; j < input.Length; j++)
                        {
                            k = input[j] - Network.NeuronsMap[x][y].Weights[j];
                            Network.NeuronsMap[x][y].Weights[j] += gauss * k;
                        }
                    }
                }
            }
        }

        private static double Distance(Neuron neuron, double[] input)
        {
            double sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                sum += Math.Abs(input[i] - neuron.Weights[i]);
                //sum += Math.Pow(input[i] - neuron.Weights[i], 2);
            }

            return sum;
        }

        private Point WinnerNeuron(double[] input)
        {
            int x = 0, y = 0;

            if (Network.NeuronsMap.Count > 0)
            {
                double minDistance = Distance(Network.NeuronsMap[0][0], input);
                for (int i = 0; i < Network.NeuronsMap.Count; i++)
                    for (int j = 0; j < Network.NeuronsMap[i].Length; j++)
                    {
                        double distance = Distance(Network.NeuronsMap[i][j], input);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            x = i;
                            y = j;
                        }
                    }
            }
            {
                
            }
            return (new Point(x, y));
        }

        private double GaussFunction(double k)
        {
            double sigma;
            //double sigma = -0.01 * k+ 2;
            //double sigma = Math.Exp(-k/(1000/Math.Log(270)));
            //double sigma = Math.Exp(-k / 5);
            //double sigma = 5 * Math.Sqrt(Network.NeuronsMap.Count * Network.NeuronsMap[0].Length) / Math.Sqrt(k);
            //sigma = Math.Exp(-k * k / (2 * sigma * sigma));

            //if (k != 0)
            //{
                sigma = 1 / (k * 1000 + 7000);
            /*}
            else
            {
                sigma = 1;
            }*/
            return sigma;
        }
    }
}
