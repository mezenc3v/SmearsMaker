using System;
using System.Collections.Generic;
using SmearTracer.Segmentation;

namespace SmearTracer.Concatenation
{
	public class BrushStroke
	{
		public List<IObject> Objects;
		public int Width;

		public BrushStroke()
		{
			Objects = new List<IObject>();
		}

		public double GetLength()
		{
			var length = 0d;
			for (int i = 1; i < Objects.Count; i++)
			{
				length += Distance(Objects[i - 1].Centroid.Position, Objects[i].Centroid.Position);
			}

			return length;
		}

		private static double Distance(System.Windows.Point first, System.Windows.Point second)
		{
			var sum = Math.Pow(first.X - second.X, 2);
			sum += Math.Pow(first.Y - second.Y, 2);
			return Math.Sqrt(sum);
		}
	}
}