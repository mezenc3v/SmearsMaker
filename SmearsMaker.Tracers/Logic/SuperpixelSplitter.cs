using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Logic
{
	public class SuperpixelSplitter : Splitter
	{
		public SuperpixelSplitter(int size) : base(size)
		{
		}

		protected override Point GetCentroid(Segment superPixel)
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

		protected override IEnumerable<System.Windows.Point> PlacementCenters(double diameter, Segment segment)
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

		protected override int NearestCentroid(Point pixel, IReadOnlyList<Segment> superPixels)
		{
			var index = 0;
			var min = Utils.ManhattanDistance(superPixels[0].Centroid, pixel);
			for (int i = 0; i < superPixels.Count; i++)
			{
				var distance = Utils.ManhattanDistance(superPixels[i].Centroid, pixel);
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