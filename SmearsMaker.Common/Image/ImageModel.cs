using System;
using System.Windows.Media.Imaging;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Helpers;

namespace SmearsMaker.Common.Image
{
	public class ImageModel : IDisposable
	{
		public int Width => Image.PixelWidth;
		public int Height => Image.PixelHeight;

		public PointCollection Points => _points ?? (_points = ImageHelper.ConvertToPixels(Image));

		public BitmapSource Image { get; }

		private PointCollection _points;

		public ImageModel(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");
		}

		public BitmapSource ConvertToBitmapSource(PointCollection points, string layer)
		{
			return ImageHelper.ConvertRgbToBitmap(Image, points, layer);
		}

		public void Dispose()
		{
			_points?.Clear();
		}
	}
}