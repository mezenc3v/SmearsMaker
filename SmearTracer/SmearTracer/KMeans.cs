using System;
using System.Collections.Concurrent;
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

        public List<Pixel> Compute(List<Pixel> data, int maxIteration)
        {
            for(int i = 0; i < Clusters.Length; i++)
            {
                Clusters[i] = new Cluster(data[0].Data.Length);
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

        private void UpdateMeans(List<Pixel> data)
        {
            foreach (Cluster cluster in Clusters)
            {
                cluster.Data = new List<Pixel>();
            }
            Parallel.ForEach(data, d =>
            {
                int index = NearestCentroid(d.Data);
                lock (data)
                {
                    Clusters[index].Data.Add(d);
                }          
            });
        } 

        private void InitialCentroids(List<Pixel> data)
        {
            //double[][] sortedArray = data;
            List<Pixel> sortedArray = data.OrderBy(p=>p.Data.Sum()).ToList();
            int step = (data.Count) / (CountClusters + 1);

            for (int i = 0; i < Clusters.Length; i++)
            {
                Clusters[i].Centroid = sortedArray[i * step / 2].Data;
            }
        }

        private void UpdateCentroids()
        {
            foreach (var cluster in Clusters)
            {
                double[] centroid = new double[Clusters[0].Centroid.Length];
                if (cluster.Data.Count > 0)
                {
                    foreach (Pixel data in cluster.Data)
                    {
                        for (int j = 0; j < data.Data.Length; j++)
                        {
                            centroid[j] += data.Data[j];
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

        private List<Pixel> ApplyCentroids(List<Pixel> data)
        {
            List<Pixel> results = new List<Pixel>(data);

            foreach (var cluster in Clusters)
            {
                List<Pixel> dataCluster = cluster.Data.OrderByDescending(c => c.Data.Max()).ToList();
                int max = 0;
                int indexOfMax = 0;
                int currentMax = 0;

                for (int i = 0; i < cluster.Data.Count - 1; i++)
                {
                    if (Math.Abs(Distance(dataCluster[i].Data, dataCluster[i+1].Data)) < _precision)
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
                cluster.LastCentroid = cluster.Data[indexOfMax].Data;
            }
            for (int i = 0; i < data.Count; i++)
            {
                int index = NearestCentroid(data[i].Data);
                results[i].Data = Clusters[index].LastCentroid;
            }

            return results;
        }

        private int NearestCentroid(double[] data)
        {
            int index = 0;
            double min = Distance(data, Clusters[0].Centroid);
            for (int i = 0; i < Clusters.Length; i++)
            {
                double distance = Distance(data, Clusters[i].Centroid);

                if (min > distance)
                {
                    min = distance;
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
