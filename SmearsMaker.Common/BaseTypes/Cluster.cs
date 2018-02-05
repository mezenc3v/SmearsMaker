using System.Collections.Generic;

namespace SmearsMaker.Common.BaseTypes
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
