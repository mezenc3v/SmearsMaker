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

		public BitmapSource GetDifference(BitmapSource image, string layer)
		{
			if (image == null)
			{
				throw new NullReferenceException(nameof(image));
			}

			var diffCollection = new PointCollection();
			var secondModel = new ImageModel(image);

			for (int i = 0; i < Points.Count; i++)
			{
				var firstArray = Points[i].Pixels[layer].Sum;
				var secondArray = secondModel.Points[i].Pixels[Layers.Original].Sum;
				var diffArray = new float[Points[i].Pixels[layer].Length];
				for (int j = 0; j < diffArray.Length; j++)
				{
					diffArray[j] = 255 - firstArray - secondArray;
				}
				var pixel = new Pixel(diffArray);
				var diffPoint = new Point(Points[i].Position);
				diffPoint.Pixels.AddPixel(Layers.Original, pixel);
				diffCollection.Add(diffPoint);
			}

			return ImageHelper.ConvertRgbToBitmap(Image, diffCollection, Layers.Original);
		}

		public BitmapSource GetDifference(string firstLayer, string secondLayer)
		{
			if (!Points.Layers.Contains(firstLayer))
			{
				throw new ArgumentOutOfRangeException(nameof(firstLayer));
			}
			if (!Points.Layers.Contains(secondLayer))
			{
				throw new ArgumentOutOfRangeException(nameof(secondLayer));
			}

			var diffCollection = new PointCollection();
			diffCollection.Addlayer(firstLayer);

			foreach (var point in Points)
			{
				var firstArray = point.Pixels[firstLayer].Data;
				var secondArray = point.Pixels[secondLayer].Data;
				var diffArray = new float[firstArray.Length];
				for (int i = 0; i < diffArray.Length; i++)
				{
					diffArray[i] = 255 - firstArray[i] - secondArray[i];
				}
				var pixel = new Pixel(diffArray);
				var diffPoint = new Point(point.Position);
				diffPoint.Pixels.AddPixel(firstLayer, pixel);
				diffCollection.Add(diffPoint);
			}

			return ImageHelper.ConvertRgbToBitmap(Image, diffCollection, firstLayer);
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