using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class Kmeans
    {
        private readonly List<Cluster> _clusters;

        private readonly double _precision;
        private readonly int _maxIteration;
        private readonly List<IUnit> _units;

        public Kmeans(int clustersCount, double precision, int dataFormatSize, List<IUnit> units, int maxIteration)
        {
            _clusters = new List<Cluster>();
            for (int i = 0; i < clustersCount; i++)
            {
                _clusters.Add(new Cluster(dataFormatSize));
            }
            _precision = precision;
            _maxIteration = maxIteration;
            _units = units;
        }

        private void Update()
        {
            var smallData = new List<IUnit>();

            for (int i = 0; i < _clusters.Count; i++)
            {
                if (_clusters[i].Data.Count == 0)
                {
                    smallData.AddRange(_clusters[i].Data);
                    _clusters.Remove(_clusters[i]);
                }
            }
            Parallel.ForEach(smallData, d =>
            {
                var index = NearestCentroid(d.Data);
                lock (smallData)
                {
                    _clusters[index].Data.Add(d);
                }
            });
        }

        private void UpdateMeans()
        {
            foreach (var cluster in _clusters)
            {
                cluster.Data = new List<IUnit>();
            }
            Parallel.ForEach(_units, d =>
            {
                var index = NearestCentroid(d.Data);
                lock (_units)
                {
                    _clusters[index].Data.Add(d);
                }
            });

            _clusters.RemoveAll(c => c.Data.Count == 0);
        }

        private void InitialCentroids()
        {
            var sortedArray = _units.OrderBy(p => p.Data.Sum()).ToList();
            var step = _units.Count / _clusters.Count;

            for (int i = 0; i < _clusters.Count; i++)
            {
                _clusters[i].Centroid = sortedArray[i * step / 2].Data;
            }
        }

        private void UpdateCentroids()
        {
            Parallel.ForEach(_clusters, cluster =>
            {
                var centroid = new double[_clusters[0].Centroid.Length];

                foreach (var data in cluster.Data)
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

        private int NearestCentroid(IReadOnlyList<double> data)
        {
            var index = 0;
            var min = Distance(data, _clusters[0].Centroid);
            for (int i = 0; i < _clusters.Count; i++)
            {
                var distance = Distance(data, _clusters[i].Centroid);

                if (min > distance)
                {
                    min = distance;
                    index = i;
                }
            }
            return index;
        }

        private static double Distance(IReadOnlyList<double> leftVector, IReadOnlyList<double> rightVector)
        {
            double dictance = 0;

            for (int i = 0; i < leftVector.Count; i++)
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

        public List<Cluster> Clustering()
        {
            InitialCentroids();

            double delta;
            var counter = 0;
            do
            {
                delta = 0;
                UpdateMeans();
                UpdateCentroids();
                foreach (var cluster in _clusters)
                {
                    delta += Distance(cluster.Centroid, cluster.LastCentroid);
                }
                counter++;
            }
            while (delta > _precision && counter < _maxIteration);

            Update();

            return _clusters;
        }
    }
}
