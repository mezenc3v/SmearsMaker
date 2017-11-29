using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace SmearTracer.Model.Helpers
{
	public class ImageHelper
	{
		private const int DataFormatSize = 4;
		public static byte ConvertToGrayScale(byte r, byte g, byte b)
		{
			//var grayscale = (byte)(0.2126 * r + 0.7152 * g + 0.0722 * b);
			var grayscale = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
			return grayscale;
		}

		public static float[] ConvertGrayScaleToRgb(float oldGrsc, float newGrsc, float[] argb)
		{
			var newArgb = new float[argb.Length];
			var ratio = newGrsc / oldGrsc;
			newArgb[0] = ratio * argb[0];
			newArgb[1] = ratio * argb[1];
			newArgb[2] = ratio * argb[2];

			return newArgb;
		}
		public static List<Point> ConvertToPixels(BitmapSource source)
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

					//var rgbArray = new float[] {  ConvertToGrayScale(data[idx], data[idx + 1], data[idx + 2]) };
					//var rgbArray = new float[] { data[idx], data[idx + 1], data[idx + 2]};
					var rgbArray = new float[] { data[idx], data[idx + 1], data[idx + 2], data[idx + 3] };
					//var rgbArray = new float[] { data[idx + 1], data[idx + 2], data[idx + 3] };
					var pixel = new Pixel(rgbArray);
					var point = new Point(pixel, x, y);
					inputData.Add(point);
				}
			}
			return inputData;
		}

		public static BitmapSource ConvertGrayScaleToRgbBitmap(BitmapSource source, List<Point> points)
		{
			var stride = source.PixelWidth * DataFormatSize;
			var size = source.PixelHeight * stride;
			var data = new byte[size];

			foreach (var pixel in points)
			{
				var indexPixel = (int)(pixel.Position.Y * stride + DataFormatSize * pixel.Position.X);
				var orig = pixel.Original.Data;
				var dataArray = pixel.Filtered.Data;
				var newArray = ConvertGrayScaleToRgb(ConvertToGrayScale((byte)orig[0], (byte)orig[1], (byte)orig[2]),
					dataArray[0], orig);
				data[indexPixel] = (byte)newArray[0];
				data[indexPixel + 1] = (byte)newArray[1];
				data[indexPixel + 2] = (byte)newArray[2];
				data[indexPixel + 3] = 255;
			}
			var image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
				source.DpiY, source.Format, source.Palette, data, stride);

			return image;
		}
		public static BitmapSource ConvertRgbToRgbBitmap(BitmapSource source, List<Point> points)
		{
			var stride = source.PixelWidth * DataFormatSize;
			var size = source.PixelHeight * stride;
			var data = new byte[size];

			foreach (var pixel in points)
			{
				if (pixel == null)
				{
					//hack
					continue;
				}

				var indexPixel = (int)(pixel.Position.Y * stride + DataFormatSize * pixel.Position.X);
				var dataArray = pixel.Filtered.Data;
				for (int i = 0; i < dataArray.Length; i++)
				{
					data[indexPixel + i] = (byte)dataArray[i];
				}
			}
			var image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
				source.DpiY, source.Format, source.Palette, data, stride);

			return image;
		}
	}
}
