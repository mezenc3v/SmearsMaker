using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.Segmenting
{
	public class Segment
	{
		public Segment()
		{
			Data = new List<Point>();
		}

		public Segment(Point centroid) : this()
		{
			Centroid = centroid;
		}

		public Segment(int width, int height) : this()
		{
			Centroid = new Point((double)width / 2, (double)height / 2);
			MaxX = new System.Windows.Point(width, height);
			MaxY = new System.Windows.Point(width, height);
			MinX = new System.Windows.Point(0, 0);
			MinY = new System.Windows.Point(0, 0);
		}

		public Point Centroid { get; set; }
		public List<Point> Data { get; set; }
		public System.Windows.Point MinX { get; set; }
		public System.Windows.Point MinY { get; set; }
		public System.Windows.Point MaxX { get; set; }
		public System.Windows.Point MaxY { get; set; }
	}
}