using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.Clustering
{
	public sealed class KmeansClassic : Kmeans
	{
		public KmeansClassic(int clustersCount, double precision, List<Point> points, int maxIteration) : base(clustersCount, precision, points, maxIteration)
		{
		}

		public override List<Point> GetClusteredPoints()
		{
			var clusteringPoints = new List<Point>();

			foreach (var cluster in Clusters)
			{
				var centr = cluster.Centroid.Data;
				foreach (var data in cluster.Data)
				{
					var point = new Point(data);
					point.Pixels[Layers.Filtered] = new Pixel(centr);
					clusteringPoints.Add(point);
				}
			}

			return clusteringPoints;
		}

		protected override double Distance(IReadOnlyList<double> left, IReadOnlyList<double> right)
		{
			double dictance = 0;

			for (int i = 0; i < left.Count; i++)
			{
				var d = left[i] - right[i];
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

		protected override void FillCentroidsWithInitialValues()
		{
			var sortedArray = Points.OrderBy(p => p.Pixels[Layers.Filtered].Sum).ToArray();
			var step = Points.Count / Clusters.Count;

			for (int i = 0; i < Clusters.Count; i++)
			{
				var point = sortedArray[i * step / 2];

				Clusters[i].Centroid = point.Pixels[Layers.Filtered];
			}
		}
		protected override void UpdateCentroid(Cluster cluster)
		{
			var newCentroid = new float[cluster.Centroid.Length];

			foreach (var data in cluster.Data)
			{
				var dataArray = data.Pixels[Layers.Filtered].Data;
				for (int i = 0; i < newCentroid.Length; i++)
				{
					newCentroid[i] += dataArray[i];
				}
			}

			for (int i = 0; i < newCentroid.Length; i++)
			{
				newCentroid[i] /= cluster.Data.Count;
			}

			cluster.LastCentroid = cluster.Centroid;
			cluster.Centroid = new Pixel(newCentroid);
		}

		protected override int NearestCentroid(Pixel pixel)
		{
			var index = 0;
			var min = pixel.Distance(Clusters[0].Centroid);
			for (int i = 0; i < Clusters.Count; i++)
			{
				var distance = pixel.Distance(Clusters[i].Centroid);

				if (min > distance)
				{
					min = distance;
					index = i;
				}
			}
			return index;
		}
	}
}