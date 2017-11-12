using System.Collections.Generic;
using SmearsMaker.Model;
using System.Linq;

namespace SmearsMaker.Filtering
{
	public class MedianFilter
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

		public void Filter(ImageModel model)
		{
			var units = model.Points;
			for (int coordX = 0; coordX < _width; coordX++)
			{
				for (int coordY = 0; coordY < _height; coordY++)
				{
					var mask = GetMask(units, coordX, coordY);
					var pos = coordX * _height + coordY;
					var median = mask.OrderByDescending(v => v.Sum).ToArray()[mask.Count / 2].Data;
					units[pos].SetFilteredPixel(median);
				}
			}
		}
		private List<Pixel> GetMask(List<Point> units, int x, int y)
		{
			var mask = new List<Pixel>();

			for (int coordMaskX = x - _rank; coordMaskX <= x + _rank; coordMaskX++)
			{
				for (int coordMaskY = y - _rank; coordMaskY <= y + _rank; coordMaskY++)
				{
					var idx = coordMaskX * _height + coordMaskY;
					if (idx < _height * _width && coordMaskX >= 0 && coordMaskY >= 0)
					{
						mask.Add(units[idx].Original);
					}
				}
			}
			return mask;
		}
	}
}