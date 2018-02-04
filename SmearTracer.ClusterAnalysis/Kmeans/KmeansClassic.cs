using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Model;

namespace SmearTracer.ClusterAnalysis.Kmeans
{
	public sealed class KmeansClassic : Kmeans
	{
		public KmeansClassic(int clustersCount, double precision, ImageModel model, int maxIteration) : base(clustersCount, precision, model, maxIteration)
		{
		}

		public override List<Point> GetClusteredPoints()
		{
			var clusteringPoints = new List<Point>();

			foreach (var cluster in Clusters)
			{
				var centr = cluster.Centroid;

				foreach (var data in cluster.Data)
				{
					var pixel = new Pixel(centr);
					var point = new Point(data.Position.X, data.Position.Y);
					point.Pixels.AddPixel(Consts.Original, data.Pixels[Consts.Original]);
					point.Pixels[Consts.Filtered] = new Pixel(pixel.Data);
					clusteringPoints.Add(point);
				}
			}

			return clusteringPoints;
		}

		public List<Point> GetAveragePoints()
		{
			var clusteringPoints = new List<Point>();

			foreach (var cluster in Clusters)
			{
				var average = new float[cluster.Data[0].Pixels[Consts.Original].Length];
				foreach (var point in cluster.Data)
				{
					var data = point.Pixels[Consts.Original].Data;
					for (int i = 0; i < point.Pixels[Consts.Original].Length; i++)
					{
						average[i] += data[i];
					}
				}
				for (int i = 0; i < cluster.Data[0].Pixels[Consts.Original].Length; i++)
				{
					average[i] /= cluster.Data.Count;
				}

				foreach (var data in cluster.Data)
				{
					var point = new Point(data.Position.X, data.Position.Y);
					point.Pixels.AddPixel(Consts.Original, data.Pixels[Consts.Original]);
					point.Pixels[Consts.Filtered] = new Pixel(average);
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
			var sortedArray = Points.OrderBy(p => p.Pixels[Consts.Filtered].Sum).ToArray();
			var step = Points.Count / Clusters.Count;

			for (int i = 0; i < Clusters.Count; i++)
			{
				var point = sortedArray[i * step / 2];

				Clusters[i].Centroid = point.Pixels[Consts.Filtered];
			}
		}
		protected override void UpdateCentroid(Cluster cluster)
		{
			var newCentroid = new float[cluster.Centroid.Length];

			foreach (var data in cluster.Data)
			{
				var dataArray = data.Pixels[Consts.Filtered].Data;
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