using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.Tracers.Extentions;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Logic
{
	public class SuperpixelSplitter : ISplitter
	{
		private readonly int _length;
		private readonly IProgress _progress;
		public SuperpixelSplitter(IProgress progress, int length)
		{
			_length = length;
			_progress = progress;
		}

		public virtual List<Segment> Splitting(Segment segment)
		{
			segment.Points.Addlayer(Layers.SuperPixels);
			//spliting complex segment into superPixels
			var data = segment.Points;
			var samples = PlacementCenters(_length, segment);
			//Search for winners and distribution of data
			_progress.NewProgress("Создание суперпикселей", 0, data.Count);
			Parallel.ForEach(data, unit =>
			{
				var winner = NearestCentroid(unit, samples);
				_progress.Update(1);
				lock (samples)
				{
					winner.Points.Add(unit);
				}
			});
			//Deleting empty cells and cells with small data count

			var segments = samples.Values.Where(superPixel => superPixel.Points.Count > 0).ToList();
			segments.ForEach(s =>
				{
					s.Points.Addlayer(Layers.Original);
					s.Points.Addlayer(Layers.Gradient);
					s.Points.Addlayer(Layers.SuperPixels);
					s.Points.Addlayer(Layers.Curves);
					s.Points.Addlayer(Layers.Filtered);
				}
			);
			return segments;
		}

		protected Dictionary<System.Windows.Point, Segment> PlacementCenters(double diameter, Segment segment)
		{
			var samplesData = new Dictionary<System.Windows.Point, Segment>();

			var (minx, miny, maxx, maxy) = segment.GetExtremums();

			var firstPoint = new System.Windows.Point(minx, miny);
			var secondPoint = new System.Windows.Point(maxx, maxy);
			var widthCount = (int)(maxx - minx);
			var heightCount = (int)(maxy - miny);

			if (widthCount > diameter && heightCount > diameter)
			{
				for (double i = firstPoint.X; i < secondPoint.X; i += diameter)
				{
					for (double j = firstPoint.Y; j < secondPoint.Y; j += diameter)
					{
						samplesData.Add(new System.Windows.Point(i + diameter / 2, j + diameter / 2), new Segment());
					}
				}
			}
			else
			{
				samplesData.Add(new System.Windows.Point((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2), new Segment());
			}

			return samplesData;
		}

		protected Segment NearestCentroid(Point pixel, Dictionary<System.Windows.Point,Segment> superPixels)
		{
			var nearest = superPixels.First().Value;
			var min = Utils.ManhattanDistance(superPixels.First().Key, pixel.Position);
			foreach (var superPixel in superPixels)
			{
				var distance = Utils.ManhattanDistance(superPixel.Key, pixel.Position);
				if (min > distance)
				{
					min = distance;
					nearest = superPixel.Value;
				}
			}
			return nearest;
		}
	}
}