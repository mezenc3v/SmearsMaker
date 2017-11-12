using System.Collections.Generic;
using SmearsMaker.Model;

namespace SmearsMaker.Segmentation
{
	public interface IObject
	{
		Point Centroid { get; set; }
		List<Point> Data { get; set; }

		System.Windows.Point MinX { get; set; }
		System.Windows.Point MinY { get; set; }
		System.Windows.Point MaxX { get; set; }
		System.Windows.Point MaxY { get; set; }
	}
}