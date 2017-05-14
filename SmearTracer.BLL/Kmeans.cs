using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearTracer.Model;

namespace SmearTracer.BLL
{
    public class Kmeans
    {
        public List<Cluster> Clusters;

        private readonly double _precision;

        public Kmeans(int countClusters, double precision, int dataFormatSize)
        {
            Clusters = new List<Cluster>();
            for (int i = 0; i < countClusters; i++)
            {
                Clusters.Add(new Cluster(dataFormatSize));
            }
            _precision = precision;
        }

        public void Compute(List<Pixel> data, int maxIteration)
        {
            InitialCentroids(data);

            double delta;
            var counter = 0;
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

            Update();
        }

        private void Update()
        {
            //List<Pixel> smallData = new List<Pixel>();

            for (int i = 0; i < Clusters.Count; i++)
            {
                if (Clusters[i].Data.Count == 0)
                {
                    //smallData.AddRange(Clusters[i].ArgbArray);
                    Clusters.Remove(Clusters[i]);
                }
            }
            /*Parallel.ForEach(smallData, d =>
            {
                int index = NearestCentroid(d.ArgbArray);
                lock (smallData)
                {
                    Clusters[index].ArgbArray.Add(d);
                }
            });*/
        }

        private void UpdateMeans(List<Pixel> data)
        {
            foreach (var cluster in Clusters)
            {
                cluster.Data = new List<Pixel>();
            }
            Parallel.ForEach(data, d =>
            {
                var index = NearestCentroid(d.ArgbArray);
                lock (data)
                {
                    Clusters[index].Data.Add(d);
                }
            });

            Clusters.RemoveAll(c => c.Data.Count == 0);
        }

        private void InitialCentroids(List<Pixel> data)
        {
            var sortedArray = data.OrderBy(p => p.ArgbArray.Sum()).ToList();
            var step = data.Count / Clusters.Count;

            for (int i = 0; i < Clusters.Count; i++)
            {
                Clusters[i].Centroid = sortedArray[i * step / 2].ArgbArray;
            }
        }

        private void UpdateCentroids()
        {
            Parallel.ForEach(Clusters, cluster =>
            {
                var centroid = new double[Clusters[0].Centroid.Length];

                foreach (var data in cluster.Data)
                {
                    for (int j = 0; j < data.ArgbArray.Length; j++)
                    {
                        centroid[j] += data.ArgbArray[j];
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
            var index = 0;
            var min = Distance(data, Clusters[0].Centroid);
            for (int i = 0; i < Clusters.Count; i++)
            {
                var distance = Distance(data, Clusters[i].Centroid);

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

            for (int i = 0; i < leftVector.Length; i++)
            {
                var d = leftVector[i] - rightVector[i];
                if (d < 0)
                {
                    dictance -= d;
                }
                else
                {
                    dictance += d;
                }
            }
            return dictance;
        }
    }
}
