using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Model;

namespace SmearsMaker.Segmentation.SimpleSegmentsSplitter
{
	public class SimpleSegmentsSplitter
	{
		public SimpleSegmentsSplitter()
		{
			
		}

		public List<Segment> Split(List<Point> input)
		{
			var segments = new List<Segment>();
			var data = input.OrderBy(p => p.Position.X).ToList();

			while (data.Count > 0)
			{
				int countPrevious, countNext;
				var segment = new Segment();
				segment.Data.Add(data[0]);
				data.RemoveAt(0);

				do
				{
					var segmentData = new List<Point>();
					countPrevious = data.Count;
					foreach (var pixel in data.OrderBy(p => Distance(p, segment.Data.Last())))
					{
						if (Contains(pixel, segment.Data))
						{
							segment.Data.Add(pixel);
						}
						else
						{
							segmentData.Add(pixel);
						}
					}
					data = segmentData;
					countNext = segmentData.Count;
				} while (countPrevious != countNext);

				var dataArr = new float[segment.Data.First().Original.Length];
				var x = 0d;
				var y = 0d;
				foreach (var point in segment.Data)
				{
					x += point.Position.X;
					y += point.Position.Y;
					var currData = point.Filtered.Data;
					for (int i = 0; i < dataArr.Length; i++)
					{
						dataArr[i] += currData[i];
					}
				}

				x /= segment.Data.Count;
				y /= segment.Data.Count;

				for (int i = 0; i < dataArr.Length; i++)
				{
					dataArr[i] /= segment.Data.Count;
				}
				segment.Centroid = new Point(new Pixel(dataArr), x, y);
				segments.Add(segment);
			}

			return segments;
		}
		private static bool Contains(Point pixel, IEnumerable<Point> units)
		{
			return units.Any(unit => Math.Abs(pixel.Position.X - unit.Position.X) < 2 && Math.Abs(pixel.Position.Y - unit.Position.Y) < 2);
		}
		private static double Distance(Point left, Point right)
		{
			//var sum = Math.Pow(right.Position.X - left.Position.X, 2);
			//sum += Math.Pow(right.Position.Y - left.Position.Y, 2);
			//return Math.Sqrt(sum);

			double dictance = 0;

			var d = left.Position.X - right.Position.X;
			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			d = left.Position.Y - right.Position.Y;

			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			return dictance;
		}
	}
}