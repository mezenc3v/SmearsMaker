using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.Clustering
{
	public class Cluster : BaseShape
	{
		public float[] Centroid;
		public float[] LastCentroid;

		public Cluster()
		{
		}

		public double DistanceBeetweenCentroids => GetDistance();

		private double GetDistance()
		{
			var dist = 0d;
			for (int i = 0; i < Centroid.Length; i++)
			{
				dist += Distance(Centroid[i], LastCentroid[i]);
			}

			return dist;
		}

		private static float Distance(float first, float second)
		{
			var distance = first - second;
			if (distance < 0)
			{
				return -distance;
			}
			return distance;
		}
	}
}
