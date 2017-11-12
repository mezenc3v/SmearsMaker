using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SmearsMaker.Model.Helpers;

namespace SmearsMaker.Model
{
	public enum ColorModel
	{
		Argb, Rgb, GrayScale, Cmyk
	}
	public class ImageModel
	{
		#region fields

		private ColorModel _colorModel;
		private readonly int _width;
		private readonly int _height;
		#endregion
		public List<Point> Points { get; }

		public static ImageModel ConvertBitmapToImage(BitmapSource bmp, ColorModel colorModel)
		{
			var original = ImageHelper.ConvertToPixels(bmp);
			return new ImageModel(bmp.PixelWidth, bmp.PixelHeight, original, colorModel);
		}
		public BitmapSource ConvertImageToBitmap(BitmapSource bmpSource)
		{
			switch (_colorModel)
			{
				case ColorModel.Argb: return ImageHelper.ConvertRgbToRgbBitmap(bmpSource, Points);
				case ColorModel.GrayScale: return ImageHelper.ConvertGrayScaleToRgbBitmap(bmpSource, Points);
				case ColorModel.Rgb:
					break;
				case ColorModel.Cmyk:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			throw new ArgumentOutOfRangeException();
		}
		public void ChangeColorModel(ColorModel model)
		{
			_colorModel = model;

			if (model == ColorModel.GrayScale)
			{
				foreach (var point in Points)
				{
					var data = point.Filtered.Data;
					var newArray = new float[] { ImageHelper.ConvertToGrayScale((byte)data[0], (byte)data[1], (byte)data[2]) };
					point.SetFilteredPixel(newArray);
				}
			}
		}

		#region ctors
		public ImageModel(ImageModel image) : this(image._width, image._height, image.Points, image._colorModel)
		{
		}

		private ImageModel(int width, int height, IReadOnlyList<Point> points, ColorModel colorModel) : this(width, height, colorModel)
		{
			Points = new List<Point>();

			foreach (var point in points)
			{
				Points.Add(new Point(point));
			}
		}

		private ImageModel(int width, int height, ColorModel colorModel)
		{
			_colorModel = colorModel;
			_width = width;
			_height = height;
		}
		#endregion
	}
}
