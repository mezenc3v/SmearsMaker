using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmearTracer
{
    public class SmearNetwork
    {
        private const double ToLerance = 10;

        public List<Smear> Network { get; set; }

        public SmearNetwork(int length, List<Pixel> inputData)
        {
            Network = new List<Smear>();
            IEnumerable<Pixel> samples = SamplesForNetwork(inputData, length);
            foreach (var sample in samples)
            {
                Smear smear = new Smear(sample);
                Network.Add(smear);
            }
        }

        public SmearNetwork(List<Smear> net)
        {
            Network = net;
        }

        private IEnumerable<Pixel> SamplesForNetwork(List<Pixel> data, int length)
        {
            List<Pixel> sortedData = data.OrderBy(d=>d.X).ThenBy(d=>d.Y).ToList();
            List<Pixel> samplesData = new List<Pixel>();
            int step = data.Count / length;

            for (int i = 0; i < length; i++)
            {
                samplesData.Add(sortedData[i * step]);
            }
            return samplesData;
        }

        private IEnumerable<Pixel> Winner(List<Pixel> data, int winnersmearPoint)
        {
            List<Pixel> winnerData = new List<Pixel>();
            Parallel.ForEach(data, pixel =>
            {
                if (winnersmearPoint == Winnersmear(pixel))
                {
                    lock (data)
                    {
                        winnerData.Add(pixel);
                    }
                }
            });
            return winnerData;
        }

        public void Learning(List<Pixel> inputData)
        {
            foreach (var smear in Network)
            {
                if (smear.Data.Count > 0)
                {
                    double x = 0;
                    double y = 0;
                    foreach (var data in smear.Data)
                    {
                        x += data.X;
                        y += data.Y;
                    }
                    x /= smear.Data.Count;
                    y /= smear.Data.Count;

                    smear.X = x;
                    smear.Y = y;
                }
            }
            //applyng data changes
            for (int i = Network.Count - 1; i >= 0; i--)
            {
                Network[i].Data.AddRange(Winner(inputData, i));

                Network[i].Data = Network[i].Data.OrderBy(d=>d.X).ThenBy(d=>d.Y).Distinct().ToList();
            }

            foreach (var smear in Network)
            {
                double x = 0;
                double y = 0;
                foreach (var data in smear.Data)
                {
                    x += data.X;
                    y += data.Y;
                }
                x /= smear.Data.Count;
                y /= smear.Data.Count;

                smear.X = x;
                smear.Y = y;
            }
        }

        private static double Distance(Smear smear, double x, double y)
        {
            double sum = Math.Pow(x - smear.X, 2);
            sum += Math.Pow(y - smear.Y, 2);
            return Math.Sqrt(sum);
        }

        private static double Distance(double[] data1, double[] data2)
        {
            double distance = 0;
            for (int i = 0; i < data1.Length; i++)
            {
                distance += Math.Abs(data1[i] - data2[i]);
            }
            return distance;
        }

        private int Winnersmear(Pixel pixel)
        {
            int index = 0;

            if (Network.Count > 0)
            {
                double minDistance = Distance(Network[0], pixel.X, pixel.Y);
                for (int i = 1; i < Network.Count; i++)
                {
                    //if (Distance(Network[i].AverageData, pixel.Data) < ToLerance)
                    //{
                        double distance = Distance(Network[i], pixel.X, pixel.Y);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            index = i;
                        }
                    //}
                }
            }
            return index;
        }
    }
}
