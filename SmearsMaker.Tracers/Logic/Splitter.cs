using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.Logic
{
	public abstract class Splitter
	{
		protected readonly int Size;

		protected Splitter(int size)
		{
			Size = size;
		}

		public List<Segment> Splitting(Segment segment)
		{
			var maxDiameter = Math.Sqrt(Size);

			//spliting complex segment into superPixels
			var data = segment.Data;
			var superPixelsList = new List<Segment>();

			var samples = PlacementCenters(maxDiameter, segment);
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

		protected abstract Point GetCentroid(Segment superPixel);
		protected abstract int NearestCentroid(Point pixel, IReadOnlyList<Segment> superPixels);
		protected abstract IEnumerable<System.Windows.Point> PlacementCenters(double diameter, Segment segment);
	}
}