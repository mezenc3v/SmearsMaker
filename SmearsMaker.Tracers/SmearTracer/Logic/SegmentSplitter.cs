using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.SmearTracer.Logic
{
	public static class SegmentSplitter
	{
		public static List<Segment> Split(List<Point> input)
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
					foreach (var pixel in data.OrderBy(p => Utils.ManhattanDistance(p.Position, segment.Data.Last().Position)))
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

				var dataArr = new float[segment.Data.First().Pixels[Layers.Original].Length];
				var x = 0d;
				var y = 0d;
				foreach (var point in segment.Data)
				{
					x += point.Position.X;
					y += point.Position.Y;
					var currData = point.Pixels[Layers.Filtered].Data;
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
				segment.Centroid = new Point(x, y);
				segment.Centroid.Pixels.AddPixel(Layers.Original, new Pixel(dataArr));
				segments.Add(segment);
			}

			return segments;
		}
		private static bool Contains(Point pixel, IEnumerable<Point> units)
		{
			return units.Any(unit => Math.Abs(pixel.Position.X - unit.Position.X) < 2 && Math.Abs(pixel.Position.Y - unit.Position.Y) < 2);
		}
	}
}