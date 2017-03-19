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

            foreach (Pixel pixel in segments)
            {
                Point winnerPoint = WinnerNeuron(pixel.Coordinates);
                Color color = Network.NeuronsMap[(int) winnerPoint.X][(int) winnerPoint.Y].Color;
                int index = (int)(pixel.Coordinates[0] * heightImage + pixel.Coordinates[1]);
                data[index][0] = color.R;
                data[index][1] = color.G;
                data[index][2] = color.B;
                data[index][3] = color.A;
            }

            return paintedDataimage;
        }

        public List<double[]> Winner(double[][] data, int coordX, int coordY)
        {
            List<double[]> winnerData = new List<double[]>();
            Point winnerNeuronPoint = new Point(coordX,coordY);

            for (int i = 0; i < data.GetLength(0); i++)
            {
                if (winnerNeuronPoint == WinnerNeuron(data[i]))
                {
                    winnerData.Add(data[i]);
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
                sum += Math.Pow(input[i] - neuron.Weights[i], 2);
            return Math.Sqrt(sum);
        }

        private Point WinnerNeuron(double[] input)
        {
            double minDistance = Distance(Network.NeuronsMap[0][0], input);
            int x = 0, y = 0;
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
            return (new Point(x, y));
        }

        private double GaussFunction(double k)
        {
            double sigma;
            //double sigma = -0.01 * number + 2;
            //double sigma = Math.Exp(-number/(1000/Math.Log(270)));
            //double sigma = Math.Exp(-number / 5);
            //double sigma = 5 * Math.Sqrt(countNeurons) / Math.Sqrt(k);
            //sigma = Math.Exp(-k * k / (2 * sigma * sigma));

            if (k != 0)
            {
                sigma = 1 / (k * 1000 + 7000);
            }
            else
            {
                sigma = 1;
            }
            return sigma;
        }
    }
}
