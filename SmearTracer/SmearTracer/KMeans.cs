using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SmearTracer
{
    public class KMeans
    {
        public Cluster[] Clusters{ get; private set; }
        public int CountClusters { get; private set; }
        private readonly double _precision;

        public KMeans (int countClusters, double precision)
        {
            Clusters = new Cluster[countClusters];

            CountClusters = countClusters;
            _precision = precision;
        }

        public double[][] Compute(double[][] data, int maxIteration)
        {

            for(int i = 0; i < Clusters.Length; i++)
            {
                Clusters[i] = new Cluster(data[0].Length);
            }

            InitialCentroids(data);

            double delta;
            int counter = 0;
            do
            {
                delta = 0;
                UpdateMeans(data);
                UpdateCentroids();
               
                foreach (var cluster in Clusters)
                {
                    delta += Distance(cluster.Centroid, cluster.LastCentroid);
                }
                counter++;
            }
            while (delta > _precision && counter < maxIteration);

            data = ApplyCentroids(data);

            return data;
        }

        private void UpdateMeans(double[][] data)
        {
            foreach (Cluster cluster in Clusters)
            {
                cluster.Data = new List<double[]>();
            }
            for (int i = 0; i < data.GetLength(0); i++)
            {
                int index = NearestCentroid(data[i]);
                Clusters[index].Data.Add(data[i]);              
            }
        } 

        private void InitialCentroids(double[][] data)
        {
            //double[][] sortedArray = data;
            double[][] sortedArray = data.OrderBy(p=>p.Sum()).ToArray();
            int step = (data.GetLength(0)) / (CountClusters + 1);

            for (int i = 0; i < Clusters.Length; i++)
            {
                Clusters[i].Centroid = sortedArray[i * step / 2];
            }
        }

        private void UpdateCentroids()
        {
            foreach (var cluster in Clusters)
            {
                double[] centroid = new double[Clusters[0].Centroid.Length];
                if (cluster.Data.Count > 0)
                {
                    foreach (double[] data in cluster.Data)
                    {
                        for (int j = 0; j < data.Length; j++)
                        {
                            centroid[j] += data[j];
                        }

                    }

                    for (int i = 0; i < centroid.Length; i++)
                    {
                        centroid[i] /= cluster.Data.Count;
                    }

                    cluster.LastCentroid = cluster.Centroid;
                    cluster.Centroid = centroid;
                }
                else
                {
                    Clusters = Clusters.Where(c => c.Data.Count > 0).ToArray();
                    CountClusters = Clusters.Length;
                }
            }         
        }

        private double[][] ApplyCentroids(double[][] data)
        {
            double[][] results = new double[data.GetLength(0)][];

            foreach (var cluster in Clusters)
            {
                double[][] dataCluster = cluster.Data.OrderByDescending(c => c.Max()).ToArray();
                int max = 0;
                int indexOfMax = 0;
                int currentMax = 0;

                for (int i = 0; i < cluster.Data.Count - 1; i++)
                {
                    if (Math.Abs(Distance(dataCluster[i], dataCluster[i+1])) < _precision)
                    //if (dataCluster[i] == dataCluster[i + 1])
                    {
                        currentMax++;
                    }
                    else
                    {
                        if (currentMax > max)
                        {
                            max = currentMax;
                            indexOfMax = i;
                        }
                        currentMax = 0;
                    }
                }
                cluster.LastCentroid = cluster.Data[indexOfMax];
            }
            for (int i = 0; i < results.GetLength(0); i++)
            {
                int index = NearestCentroid(data[i]);
                results[i] = Clusters[index].LastCentroid;
            }

            return results;
        }

        private int NearestCentroid(double[] data)
        {
            double minDistance = Distance(data, Clusters[0].Centroid);
            int index = 0;
            for(int i = 0; i < Clusters.Length; i++)
            {
                double distance = Distance(data, Clusters[i].Centroid);

                if (minDistance > distance)
                {
                    minDistance = distance;
                    index = i;
                }             
            }
            return index;
        }

        private static double Distance(double[] leftVector, double[] rightVector)
        {
            double dictance = 0;

            for(int i = 0; i < leftVector.Length; i++)
            {
                dictance += Math.Abs(leftVector[i] - rightVector[i]);
            }
            return dictance;
        }
    }
}
