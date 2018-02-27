using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace GradientTracer.Logic
{
	public class SuperpixelSplitter
	{
		private readonly int _minSize;
		private readonly int _maxSize;
		public SuperpixelSplitter(int minSize, int maxSize, double tolerance)
		{
			_minSize = minSize;
			_maxSize = maxSize;
		}

		public List<Segment> Splitting(Segment segment)
		{
			var maxDiameter = Math.Sqrt(_maxSize);
			var minDiameter = Math.Sqrt(_minSize);

			//spliting complex segment into superPixels
			var data = segment.Data;
			var superPixelsList = new List<Segment>();

			var samples = InitilalCentroids(maxDiameter, segment);
			var superPixels = samples.Select(centroid =>
			{
				var p = new Point(centroid.X, centroid.Y);
				p.Pixels.AddPixel(Layers.Original, segment.Centroid.Pixels[Layers.Original]);
				return new Segment(p);
			}).ToList();
			//Search for winners and distribution of data
			Parallel.ForEach(data, unit =>
			{
				var winner = NearestCentroid(unit, superPixels);
				lock (superPixels)
				{
					superPixels[winner].Data.Add(unit);
				}
			});
			//Deleting empty cells and cells with small data count
			foreach (var superPixel in superPixels)
			{
				if (superPixel.Data.Count > 0)
				{
					var newCentroid = GetCentroid(superPixel);
					superPixel.Centroid = newCentroid;
					superPixelsList.Add(superPixel);
				}
			}

			return superPixelsList;
		}

		private static Point GetCentroid(Segment superPixel)
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
		private static (System.Windows.Point minx, System.Windows.Point miny, System.Windows.Point maxx, System.Windows.Point maxy) GetExtremums(Segment segment)
		{
			var points = segment.Data;

			//coordinates for compute vector

			var minX = points[0].Position.X;
			var minY = points[0].Position.Y;
			var maxX = minX;
			var maxY = minY;
			var MinX = new System.Windows.Point(points[0].Position.X, points[0].Position.Y);
			var MaxX = new System.Windows.Point(points[0].Position.X, points[0].Position.Y);
			var MinY = new System.Windows.Point(points[0].Position.X, points[0].Position.Y);
			var MaxY = new System.Windows.Point(points[0].Position.X, points[0].Position.Y);
			foreach (var data in points)
			{
				//find min and max coordinates in segment
				if (data.Position.X < minX)
				{
					minX = data.Position.X;
					MinX = new System.Windows.Point(data.Position.X, data.Position.Y);
				}
				if (data.Position.Y < minY)
				{
					minY = data.Position.Y;
					MinY = new System.Windows.Point(data.Position.X, data.Position.Y);
				}
				if (data.Position.X > maxX)
				{
					maxX = data.Position.X;
					MaxX = new System.Windows.Point(data.Position.X, data.Position.Y);
				}
				if (data.Position.Y > maxY)
				{
					maxY = data.Position.Y;
					MaxY = new System.Windows.Point(data.Position.X, data.Position.Y);
				}
			}

			return (MinX, MinY, MaxX, MaxY);
		}

		private static IEnumerable<System.Windows.Point> InitilalCentroids(double diameter, Segment segment)
		{
			var samplesData = new List<System.Windows.Point>();

			var (minx, miny, maxx, maxy) = GetExtremums(segment);

			var firstPoint = new System.Windows.Point(minx.X, miny.Y);
			var secondPoint = new System.Windows.Point(maxx.X, maxy.Y);
			var widthCount = (int)(maxx.X - minx.X);
			var heightCount = (int)(maxy.Y - miny.Y);

			if (widthCount > diameter && heightCount > diameter)
			{
				for (int i = (int)firstPoint.X; i < (int)secondPoint.X; i += (int)diameter)
				{
					for (int j = (int)firstPoint.Y; j < (int)secondPoint.Y; j += (int)diameter)
					{
						samplesData.Add(new System.Windows.Point(i + diameter / 2, j + diameter / 2));
					}
				}
			}
			else
			{
				samplesData.Add(new System.Windows.Point((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2));
			}

			return samplesData;
		}

		private static double Distance(Segment superPixel, Point pixel)
		{
			//var sum = Math.Pow(pixel.Position.X - superPixel.Centroid.Position.X, 2);
			//sum += Math.Pow(pixel.Position.Y - superPixel.Centroid.Position.Y, 2);
			//return Math.Sqrt(sum);

			double dictance = 0;

			var d = pixel.Position.X - superPixel.Centroid.Position.X;
			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			d = pixel.Position.Y - superPixel.Centroid.Position.Y;

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

		private static int NearestCentroid(Point pixel, IReadOnlyList<Segment> superPixels)
		{
			var index = 0;
			var min = Distance(superPixels[0], pixel);
			for (int i = 0; i < superPixels.Count; i++)
			{
				var distance = Distance(superPixels[i], pixel);
				if (min > distance)
				{
					min = distance;
					index = i;
				}
			}
			return index;
		}
	}
}