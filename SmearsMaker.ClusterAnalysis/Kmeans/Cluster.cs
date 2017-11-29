using System.Collections.Generic;
using SmearTracer.Model;

namespace SmearTracer.ClusterAnalysis.Kmeans
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
