using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.Filtering
{
	public class MedianFilter : IFilter
	{
		private readonly int _rank;
		private readonly int _width;
		private readonly int _height;
		public MedianFilter(int rank, int width, int height)
		{
			_rank = rank;
			_width = width;
			_height = height;
		}

		public List<Point> Filtering(List<Point> points)
		{
			var filteredPoints = new List<Point>();

			for (int coordX = 0; coordX < _width; coordX++)
			{
				for (int coordY = 0; coordY < _height; coordY++)
				{
					var mask = GetMask(points, coordX, coordY);
					var pos = coordX * _height + coordY;
					var median = mask.OrderByDescending(v => v.Sum).ToArray()[mask.Count / 2].Data;
					var clonePoint = points[pos].Clone();
					clonePoint.Pixels[Layers.Filtered] = Pixel.CreateInstance(median);
					filteredPoints.Add(clonePoint);
				}
			}
			return filteredPoints;
		}
		private List<Pixel> GetMask(IList<Point> units, int x, int y)
		{
			var mask = new List<Pixel>();

			for (int coordMaskX = x - _rank; coordMaskX <= x + _rank; coordMaskX++)
			{
				for (int coordMaskY = y - _rank; coordMaskY <= y + _rank; coordMaskY++)
				{
					var idx = coordMaskX * _height + coordMaskY;
					if (idx < _height * _width && coordMaskX >= 0 && coordMaskY >= 0)
					{
						mask.Add(units[idx].Pixels[Layers.Original]);
					}
				}
			}
			return mask;
		}
	}
}