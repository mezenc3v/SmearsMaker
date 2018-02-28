using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Common.Helpers
{
	internal class ImageHelper
	{
		private const int DataFormatSize = 4;

		internal static List<Point> ConvertToPixels(BitmapSource source)
		{
			var inputData = new List<Point>();
			var stride = source.PixelWidth * DataFormatSize;
			var size = source.PixelHeight * stride;
			var data = new byte[size];
			source.CopyPixels(data, stride, 0);
			for (int x = 0; x < source.PixelWidth; x++)
			{
				for (int y = 0; y < source.PixelHeight; y++)
				{
					var idx = y * stride + DataFormatSize * x;
					var rgbArray = new float[] { data[idx], data[idx + 1], data[idx + 2], data[idx + 3] };
					var point = new Point(x, y);
					point.Pixels.AddPixel(Layers.Original, new Pixel(rgbArray));
					inputData.Add(point);
				}
			}
			return inputData;
		}

		internal static BitmapSource ConvertRgbToBitmap(BitmapSource source, List<Point> points, string name)
		{
			var stride = source.PixelWidth * DataFormatSize;
			var size = source.PixelHeight * stride;
			var data = new byte[size];

			foreach (var pixel in points)
			{
				var indexPixel = (int)(pixel.Position.Y * stride + DataFormatSize * pixel.Position.X);
				var dataArray = pixel.Pixels[name].Data;
				for (int i = 0; i < dataArray.Length; i++)
				{
					data[indexPixel + i] = (byte)dataArray[i];
				}
			}
			var image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, source.Format, source.Palette, data, stride);

			return image;
		}
	}
}
