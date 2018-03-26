using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.Clustering
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
