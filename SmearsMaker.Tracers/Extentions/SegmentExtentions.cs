using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.Extentions
{
	public static class BaseShapeExtentions
	{
		public static bool IsSameColor(this BaseShape firstSegment, BaseShape secondSegment, double tolerance, string layer)
		{
			var distance = Math.Abs(firstSegment.GetCenter(layer).Average - secondSegment.GetCenter(layer).Average);
			return distance < tolerance;
		}

		public static BaseShape FindNearest(this BaseShape obj, IEnumerable<BaseShape> objs, string layer)
		{
			var data = obj.GetCenter(layer).Data[0];
			var result = objs.OrderBy(p => Math.Abs(data - p.GetCenter(layer).Data[0]));
			return result.First();
		}

		public static float[] GetAverageData(this BaseShape seg, string layer)
		{
			var length = seg.Points.First().Pixels[layer].Length;
			var averData = new float[length];

			foreach (var point in seg.Points)
			{
				for (int i = 0; i < length; i++)
				{
					averData[i] += point.Pixels[layer].Data[i];
				}
			}

			for (int i = 0; i < length; i++)
			{
				averData[i] /= seg.Points.Count;
			}

			return averData;
		}

		public static (double minx, double miny, double maxx, double maxy) GetExtremums(this BaseShape segment)
		{
			var points = segment.Points;

			//coordinates for compute vector
			double minX = points[0].Position.X;
			double minY = points[0].Position.Y;
			double maxX = 0;
			double maxY = 0;

			foreach (var data in points)
			{
				//find min and max coordinates in segment
				if (data.Position.X < minX)
				{
					minX = data.Position.X;
				}
				if (data.Position.Y < minY)
				{
					minY = data.Position.Y;
				}
				if (data.Position.X > maxX)
				{
					maxX = data.Position.X;
				}
				if (data.Position.Y > maxY)
				{
					maxY = data.Position.Y;
				}
			}

			return (minX, minY, maxX, maxY);
		}
	}
}