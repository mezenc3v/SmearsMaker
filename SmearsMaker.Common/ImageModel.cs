using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Helpers;

namespace SmearsMaker.Common
{
	public class ImageModel
	{
		public List<Point> Points { get; }

		public static ImageModel ConvertBitmapToImage(BitmapSource bmp)
		{
			var original = ImageHelper.ConvertToPixels(bmp);
			return new ImageModel(original);
		}
		public BitmapSource ConvertImageToBitmap(BitmapSource bmpSource)
		{
			return ImageHelper.ConvertRgbToRgbBitmap(bmpSource, Points, Consts.Filtered);
		}

		#region ctors

		private ImageModel(IEnumerable<Point> points)
		{
			Points = new List<Point>();

			foreach (var point in points)
			{
				Points.Add(new Point(point));
			}
		}

		#endregion
	}
}
