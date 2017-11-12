using System.Collections.Generic;
using SmearsMaker.Model;

namespace SmearsMaker.ClusterAnalysis.Kmeans
{
	public class Cluster
	{
		public Pixel Centroid;
		public Pixel LastCentroid;
		public List<Point> Data;

		public Cluster()
		{
			Data = new List<Point>();
		}
	}
}
