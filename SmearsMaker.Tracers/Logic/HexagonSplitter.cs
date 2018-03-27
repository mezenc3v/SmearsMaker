using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Logic
{
	public class HexagonSplitter : ISplitter
	{
		private double _inscribedRadius;
		private readonly double _circumscribedRadius;
		private readonly IProgress _progress;
		public HexagonSplitter(IProgress progress, int length)
		{
			_circumscribedRadius = length;
			_progress = progress;
		}

		public List<Segment> Splitting(Segment segment)
		{
			_inscribedRadius = Math.Sqrt(3) / 2 * _circumscribedRadius;

			//spliting complex segment into superPixels
			var data = segment.Data;
			var samples = PlacementCenters(_inscribedRadius, segment);
			var superPixels = samples.Select(row =>
			{
				return row.Select(center =>
				{
					var p = new Point(center.X, center.Y);
					p.Pixels.AddPixel(Layers.Original, segment.Centroid.Pixels[Layers.Original]);
					return new Segment(p);
				}).ToList();
			}).ToList();
			//Search for winners and distribution of data
			_progress.NewProgress("Создание суперпикселей", 0, data.Count);
			Parallel.ForEach(data, unit =>
			{
				var winner = NearestCentroid(unit, superPixels);
				_progress.Update(1);
				lock (superPixels)
				{
					winner.Data.Add(unit);
				}
			});

			var result = new List<Segment>();
			foreach (var row in superPixels)
			{
				foreach (var segment1 in row)
				{
					if (segment1.Data.Count > 0)
					{
						var newCentroid = GetCentroid(segment1);
						segment1.Centroid = newCentroid;
						result.Add(segment1);
					}
				}
			}

			return result;
		}

		protected Point GetCentroid(Segment superPixel)
		{
			var points = superPixel.Data;

			//coorditates for compute centroid
			int x = 0;
			int y = 0;
			var averageData = new float[points.First().Pixels[Layers.Original].Length];

			foreach (var point in points)
			{
				x += (int)point.Position.X;
				y += (int)point.Position.Y;
				for (int i = 0; i < averageData.Length; i++)
				{
					averageData[i] += point.Pixels[Layers.Original].Data[i];
				}
			}

			x /= points.Count;
			y /= points.Count;
			for (int i = 0; i <
							averageData.Length; i++)
			{
				averageData[i] /= points.Count;
			}

			var p = new Point(x, y);
			p.Pixels.AddPixel(Layers.Original, new Pixel(averageData));
			return p;
		}

		protected Segment NearestCentroid(Point pixel, List<List<Segment>> superPixels)
		{
			var nearest = superPixels[0][0];
			var min = Utils.SqrtDistance(superPixels[0][0].Centroid.Position, pixel.Position);
			var minIndex = 0;

			foreach (var row in superPixels)
			{
				var minY = Math.Abs(row[0].Centroid.Position.Y - pixel.Position.Y);

				if (minIndex < 0)
				{
					minIndex = 0;
				}
				for (var i = minIndex; i < row.Count; i++)
				{
					var seg = row[i];
					var dy = Math.Abs(seg.Centroid.Position.Y - pixel.Position.Y);
					if (dy <= minY)
					{
						minY = dy;
						minIndex = i - 1;
					}
					else
					{
						break;
					}

					var distance = Utils.SqrtDistance(seg.Centroid.Position, pixel.Position);
					if (min > distance)
					{
						min = distance;
						nearest = seg;
					}
				}
			}
			return nearest;
		}

		protected List<List<System.Windows.Point>> PlacementCenters(double diameter, Segment segment)
		{
			var (minx, miny, maxx, maxy) = GetExtremums(segment);

			var samplesData = new List<List<System.Windows.Point>>();

			var count = 0;
			for (double i = minx + diameter / 2; i < maxx; i += diameter)
			{
				var offset = count % 2 == 0 ? diameter : diameter / 2;
				var row = new List<System.Windows.Point>();
				for (double j = miny + offset; j < maxy; j += diameter)
				{
					row.Add(new System.Windows.Point(i, j));
				}
				samplesData.Add(row);
				count++;
			}

			return samplesData;
		}

		private static (double minx, double miny, double maxx, double maxy) GetExtremums(Segment segment)
		{
			var points = segment.Data;

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