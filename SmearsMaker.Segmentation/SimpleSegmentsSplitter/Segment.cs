using System.Collections.Generic;
using SmearsMaker.Model;

namespace SmearsMaker.Segmentation.SimpleSegmentsSplitter
{
	public class Segment : IObject
	{
		public Point Centroid { get; set; }
		public List<Point> Data { get; set; }
		public System.Windows.Point MinX { get; set; }
		public System.Windows.Point MinY { get; set; }
		public System.Windows.Point MaxX { get; set; }
		public System.Windows.Point MaxY { get; set; }

		public Segment()
		{
			Data = new List<Point>();
		}
	}
}