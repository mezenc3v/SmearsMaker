using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmearTracer
{
    public class KMeans
    {
        public List<Cluster> Clusters { get; set; }

        private readonly double _precision;

        public KMeans(int countClusters, double precision, int dataFormatSize)
        {
            Clusters = new List<Cluster>();
            for (int i = 0; i < countClusters; i++)
            {
                Clusters.Add(new Cluster(dataFormatSize));
            }
            _precision = precision;
        }

        public void Compute(List<Pixel> data, int maxIteration, int size)
        {
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

            Update(size);
        }

        private void Update(int sizeOfCluster)
        {
            List<Pixel> smallData = new List<Pixel>();

            for (int i = 0; i < Clusters.Count; i++)
            {
                if (Clusters[i].Data.Count < sizeOfCluster)
                {
                    smallData.AddRange(Clusters[i].Data);
                    Clusters.Remove(Clusters[i]);
                }
            }
            Parallel.ForEach(smallData, d =>
            {
                int index = NearestCentroid(d.Data);
                lock (smallData)
                {
                    Clusters[index].Data.Add(d);
                }
            });
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

            Clusters.RemoveAll(c => c.Data.Count == 0);
        }

        private void InitialCentroids(List<Pixel> data)
        {
            List<Pixel> sortedArray = data.OrderBy(p => p.Data.Sum()).ToList();
            int step = data.Count / Clusters.Count;

            for (int i = 0; i < Clusters.Count; i++)
            {
                Clusters[i].Centroid = sortedArray[i * step / 2].Data;
            }
        }

        private void UpdateCentroids()
        {
            Parallel.ForEach(Clusters, cluster =>
            {
                double[] centroid = new double[Clusters[0].Centroid.Length];

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
            });
        }

        private int NearestCentroid(double[] data)
        {
            int index = 0;
            double min = Distance(data, Clusters[0].Centroid);
            for (int i = 0; i < Clusters.Count; i++)
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

        private double Distance(double[] leftVector, double[] rightVector)
        {
            double dictance = 0;

            for (int i = 0; i < leftVector.Length; i++)
            {
                dictance += Math.Abs(leftVector[i] - rightVector[i]);
            }
            return dictance;
        }
    }
}
