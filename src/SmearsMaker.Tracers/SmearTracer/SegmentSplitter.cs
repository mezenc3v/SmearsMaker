using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.SmearTracer
{
	public static class SegmentSplitter
	{
		public static List<Segment> Split(BaseShape shape)
		{
			var segments = new List<Segment>();
			var data = shape.Points.OrderBy(p => p.Position.X).ToList();

			while (data.Count > 0)
			{
				int countPrevious, countNext;
				var segment = new Segment();
				segment.Points.Add(data[0]);
				data.RemoveAt(0);

				do
				{
					var segmentData = new List<Point>();
					countPrevious = data.Count;
					foreach (var pixel in data.OrderBy(p => Utils.ManhattanDistance(p.Position, segment.Points.Last().Position)))
					{
						if (Contains(pixel, segment.Points))
						{
							segment.Points.Add(pixel);
						}
						else
						{
							segmentData.Add(pixel);
						}
					}
					data = segmentData;
					countNext = segmentData.Count;
				} while (countPrevious != countNext);

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