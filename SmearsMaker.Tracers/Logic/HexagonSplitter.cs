using System;
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
			segment.Points.Addlayer(Layers.SuperPixels);
			_inscribedRadius = Math.Sqrt(3) / 2 * _circumscribedRadius;

			//spliting complex segment into superPixels
			var data = segment.Points;
			var samples = PlacementCenters(_inscribedRadius, segment);
			//Search for winners and distribution of data
			_progress.NewProgress("Создание суперпикселей", 0, data.Count);
			Parallel.ForEach(data, unit =>
			{
				var winner = NearestCentroid(unit, samples);
				lock (samples)
				{
					_progress.Update(1);
					winner.Points.Add(unit);
				}
			});
			var result = new List<Segment>();
			foreach (var row in samples)
			{
				result.AddRange(row.Where(superPixel => superPixel.Item2.Points.Count > 0).Select(s => s.Item2).ToList());
			}

			result.ForEach(s =>
				{
					s.Points.Addlayer(Layers.Original);
					s.Points.Addlayer(Layers.Gradient);
					s.Points.Addlayer(Layers.SuperPixels);
					s.Points.Addlayer(Layers.Curves);
					s.Points.Addlayer(Layers.Filtered);
				}
			);

			return result;
		}

		protected Segment NearestCentroid(Point pixel, List<List<(System.Windows.Point, Segment)>> superPixels)
		{
			var nearest = superPixels[0].First().Item2;
			var min = Utils.SqrtDistance(superPixels[0].First().Item1, pixel.Position);
			var minIndex = 0;

			foreach (var row in superPixels)
			{
				var minY = Math.Abs(row.First().Item1.Y - pixel.Position.Y);

				if (minIndex < 0)
				{
					minIndex = 0;
				}
				for (var i = minIndex; i < row.Count; i++)
				{
					var center = row[i].Item1;
					var dy = Math.Abs(center.Y - pixel.Position.Y);
					if (dy <= minY)
					{
						minY = dy;
						minIndex = i - 1;
					}
					else
					{
						break;
					}
					var distance = Utils.SqrtDistance(center, pixel.Position);
					if (min > distance)
					{
						min = distance;
						nearest = row[i].Item2;
					}
				}
			}
			return nearest;
		}

		protected List<List<(System.Windows.Point, Segment)>> PlacementCenters(double diameter, Segment segment)
		{
			var (minx, miny, maxx, maxy) = segment.GetExtremums();

			var samplesData = new List<List<(System.Windows.Point, Segment)>>();

			var count = 0;
			for (double i = minx + diameter / 2; i < maxx; i += diameter)
			{
				var offset = count % 2 == 0 ? diameter : diameter / 2;
				var row = new List<(System.Windows.Point, Segment)>();
				for (double j = miny + offset; j < maxy; j += diameter)
				{
					var segments = (p: new System.Windows.Point(i, j), s: new Segment());
					row.Add(segments);
				}
				samplesData.Add(row);
				count++;
			}

			return samplesData;
		}
	}
}