using System;
using System.Collections.Generic;
using Point = System.Windows.Point;

namespace SmearTracer.Segmentation.SuperpixelSplitter
{
	public class SuperPixel : IObject
	{
		public Model.Point Centroid { get; set; }
		public List<Model.Point> Data { get; set; }

		public Point MinX { get; set; }
		public Point MinY { get; set; }
		public Point MaxX { get; set; }
		public Point MaxY { get; set; }

	public SuperPixel(Model.Point point)
		{
			Centroid = point ?? throw new NullReferenceException("point");
			Data = new List<Model.Point>();
		}

	}
}