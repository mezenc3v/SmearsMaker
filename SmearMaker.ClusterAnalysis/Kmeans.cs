using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SmearsMaker.ClusterAnalysis
{
    public class Kmeans
    {
	    private readonly List<Cluster> _clusters;
	    private readonly double _precision;
	    private readonly int _maxIteration;
	    private readonly List<Point> _points;

	    public Kmeans(int clustersCount, double precision, int dataFormatSize, List<Point> points, int maxIteration)
	    {
		    _clusters = new List<Cluster>();
		    for (int i = 0; i < clustersCount; i++)
		    {
			    _clusters.Add(new Cluster(dataFormatSize));
		    }
		    _precision = precision;
		    _maxIteration = maxIteration;
		    _points = points;
	    }

	    public List<Cluster> Clustering()
	    {
		    InitialCentroids();

		    double delta;
		    var counter = 0;
		    do
		    {
			    UpdateMeans();
			    UpdateCentroids();
			    delta = _clusters.Sum(cluster => Distance(cluster.Centroid, cluster.LastCentroid));
			    counter++;
		    }
		    while (delta > _precision && counter < _maxIteration);

		    Update();

		    return _clusters;
	    }

		private void Update()
	    {
		    var smallData = new List<Point>();

		    for (int i = 0; i < _clusters.Count; i++)
		    {
			    if (_clusters[i].Data.Count != 0) continue;
			    smallData.AddRange(_clusters[i].Data);
			    _clusters.Remove(_clusters[i]);
		    }
		    Parallel.ForEach(smallData, d =>
		    {
			    var index = NearestCentroid(d.Filtered);
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
			    cluster.Data = new List<Point>();
		    }
		    Parallel.ForEach(_points, d =>
		    {
			    var index = NearestCentroid(d.Filtered);
			    lock (_points)
			    {
				    _clusters[index].Data.Add(d);
				    d.Cluster = _clusters[index];
			    }
		    });

		    _clusters.RemoveAll(c => c.Data.Count == 0);
	    }

	    private void InitialCentroids()
	    {
		    var sortedArray = _points.OrderBy(p => p.Filtered.RgbSum).ToList();
		    var step = _points.Count / _clusters.Count;

		    for (int i = 0; i < _clusters.Count; i++)
		    {
			    var point = sortedArray[i * step / 2];

			    _clusters[i].Centroid[0] = point.Filtered.Red;
			    _clusters[i].Centroid[1] = point.Filtered.Green;
			    _clusters[i].Centroid[2] = point.Filtered.Blue;
			}
	    }

	    private void UpdateCentroids()
	    {
		    Parallel.ForEach(_clusters, cluster =>
		    {
			    var centroid = new double[_clusters[0].Centroid.Length];

			    foreach (var data in cluster.Data)
			    {
				    centroid[0] += data.Filtered.Red;
				    centroid[1] += data.Filtered.Green;
				    centroid[2] += data.Filtered.Blue;
				}

			    for (int i = 0; i < centroid.Length; i++)
			    {
				    centroid[i] /= cluster.Data.Count;
			    }

			    cluster.LastCentroid = cluster.Centroid;
			    cluster.Centroid = centroid;
		    });
	    }

	    private int NearestCentroid(Pixel data)
	    {
		    var index = 0;
		    var min = data.Distance(_clusters[0].Centroid);
		    for (int i = 0; i < _clusters.Count; i++)
		    {
			    var distance = data.Distance(_clusters[i].Centroid);

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
	}
}
