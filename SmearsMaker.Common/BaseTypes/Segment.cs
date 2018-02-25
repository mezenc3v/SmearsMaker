using System.Collections.Generic;

namespace SmearsMaker.Common.BaseTypes
{
	public abstract class Segment
	{
		protected Segment()
		{
			Data = new List<Point>();
		}
		public Point Centroid { get; set; }
		public List<Point> Data { get; set; }
		public System.Windows.Point MinX { get; set; }
		public System.Windows.Point MinY { get; set; }
		public System.Windows.Point MaxX { get; set; }
		public System.Windows.Point MaxY { get; set; }
	}
}