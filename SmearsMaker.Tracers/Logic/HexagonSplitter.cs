using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Logic
{
	public class HexagonSplitter : ISplitter
	{
		private int _length;
		private double _inscribedRadius;
		private double _circumscribedRadius;

		public HexagonSplitter()
		{
			
		}

		public List<Segment> Splitting(Segment segment, int length)
		{
			_length = length;
			_circumscribedRadius = length;
			_inscribedRadius = Math.Sqrt(3) / 2 * _circumscribedRadius;

			//spliting complex segment into superPixels
			var data = segment.Data;
			var superPixelsList = new List<Segment>();

			var samples = PlacementCenters(_inscribedRadius, segment);
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

		protected int NearestCentroid(Point pixel, IReadOnlyList<Segment> superPixels)
		{
			var index = 0;
			var min = Utils.SqrtDistance(superPixels[0].Centroid.Position, pixel.Position);
			for (int i = 0; i < superPixels.Count; i++)
			{
				var distance = Utils.SqrtDistance(superPixels[i].Centroid.Position, pixel.Position);
				if (min > distance)
				{
					min = distance;
					index = i;
				}
			}
			return index;
		}

		protected IEnumerable<System.Windows.Point> PlacementCenters(double diameter, Segment segment)
		{
			var samplesData = new List<System.Windows.Point>();

			var (minx, miny, maxx, maxy) = GetExtremums(segment);

			var firstPoint = new System.Windows.Point(minx.X, miny.Y);
			var secondPoint = new System.Windows.Point(maxx.X, maxy.Y);

			var count = 0;
			for (int i = (int)firstPoint.X + (int)diameter / 2; i < (int)secondPoint.X; i += (int)diameter)
			{
				var offset = count % 2 == 0 ? (int) diameter : (int)diameter / 2;

				for (int j = (int)firstPoint.Y + offset; j < (int)secondPoint.Y; j += (int)diameter)
				{
					samplesData.Add(new System.Windows.Point(i, j));
				}
				count++;
			}

			return samplesData;
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
	}
}