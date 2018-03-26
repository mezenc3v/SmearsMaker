using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Helpers;

namespace SmearsMaker.Common.Image
{
	public class ImageModel
	{
		public int Width => Image.PixelWidth;
		public int Height => Image.PixelHeight;
		public List<Point> Points { get; }
		public BitmapSource Image { get; }
		public ImageModel(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");
			Points = Points = ImageHelper.ConvertToPixels(image);
		}

		public BitmapSource ConvertToBitmapSource(ICollection<Point> points, string layer)
		{
			return ImageHelper.ConvertRgbToBitmap(Image, points, layer);
		}
	}
}