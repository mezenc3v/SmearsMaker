using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Clustering;

namespace SmearsMaker.ImageProcessing
{
	public interface IClusterizer
	{
		List<Cluster> Clustering(PointCollection points);
	}
}