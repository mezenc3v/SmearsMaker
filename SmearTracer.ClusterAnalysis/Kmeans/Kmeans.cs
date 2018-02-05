using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace SmearTracer.ClusterAnalysis.Kmeans
{
	public abstract class Kmeans
	{
		protected readonly ImageModel Model;
		protected readonly List<Cluster> Clusters;
		protected readonly List<Point> Points;
		private readonly double _precision;
		private readonly int _maxIteration;

		protected Kmeans(int clustersCount, double precision, ImageModel model, int maxIteration)
		{
			Model = model;
			Clusters = new List<Cluster>();
			for (int i = 0; i < clustersCount; i++)
			{
				Clusters.Add(new Cluster());
			}
			_precision = precision;
			_maxIteration = maxIteration;
			Points = model.Points;
		}
		public List<Cluster> Clustering()
		{
			FillCentroidsWithInitialValues();

			double delta;
			var counter = 0;
			do
			{
				UpdateMeans();
				UpdateCentroids();
				delta = Clusters.Sum(cluster => cluster.Centroid.Distance(cluster.LastCentroid));
				counter++;
			}
			while (delta > _precision && counter < _maxIteration);

			MergingSmallClusters();

			return Clusters;
		}
		private void MergingSmallClusters()
		{
			var smallData = new List<Point>();

			for (int i = 0; i < Clusters.Count; i++)
			{
				if (Clusters[i].Data.Count != 0) continue;
				smallData.AddRange(Clusters[i].Data);
				Clusters.Remove(Clusters[i]);
			}
			Parallel.ForEach(smallData, d =>
			{
				var index = NearestCentroid(d.Pixels[Consts.Filtered]);
				lock (smallData)
				{
					Clusters[index].Data.Add(d);
				}
			});
		}
		private void UpdateMeans()
		{
			foreach (var cluster in Clusters)
			{
				cluster.Data = new List<Point>();
			}
			Parallel.ForEach(Points, d =>
			{
				var index = NearestCentroid(d.Pixels[Consts.Original]);
				lock (Points)
				{
					Clusters[index].Data.Add(d);
				}
			});

			Clusters.RemoveAll(c => c.Data.Count == 0);
		}
		private void UpdateCentroids()
		{
			Parallel.ForEach(Clusters, UpdateCentroid);
		}
		protected abstract void FillCentroidsWithInitialValues();
		protected abstract void UpdateCentroid(Cluster cluster);
		protected abstract int NearestCentroid(Pixel pixel);
		public abstract List<Point> GetClusteredPoints();
		protected abstract double Distance(IReadOnlyList<double> left, IReadOnlyList<double> right);
	}
}
