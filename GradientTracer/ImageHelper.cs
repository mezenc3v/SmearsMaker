using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace GradientTracer
{
	internal static class ImageHelper
	{
		internal static BitmapSource CreateRandomImage(IEnumerable<Segment> objects, string layer, ImageModel model)
		{
			var data = new List<Point>();
			foreach (var obj in objects)
			{
				var rand = GetGandomData(3).ToArray();
				obj.Data.ForEach(d =>
				{
					d.Pixels.AddPixel(layer, new Pixel(rand));
				});
				data.AddRange(obj.Data);
			}
			return model.ConvertToBitmapSource(data, layer);
		}
		internal static BitmapSource CreateImage(IEnumerable<Segment> objects, string layer, ImageModel model)
		{
			var data = new List<Point>();
			foreach (var obj in objects)
			{
				data.AddRange(obj.Data);
			}
			return model.ConvertToBitmapSource(data, layer);
		}

		internal static BitmapSource PaintImage(BitmapSource source, IEnumerable<BrushStroke> strokes, float Width)
		{
			Bitmap bitmap;
			using (var outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(source));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}

			var g = Graphics.FromImage(bitmap);
			g.Clear(Color.White);

			foreach (var stroke in strokes.OrderByDescending(s => s.Objects.Count))
			{
				var center = GetAverageData(stroke.Objects, Layers.Original);
				var color = GetColorFromArgb(center);
				
				var brush = new SolidBrush(color);
				var pen = new Pen(color)
				{
					Width = Width
				};

				var pointsF = stroke.Objects.Select(point => new PointF((float) point.Centroid.Position.X, (float) point.Centroid.Position.Y)).ToArray();

				g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);

				if (pointsF.Length > 1)
				{
					g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y, pen.Width, pen.Width);
					g.DrawLines(pen, pointsF);
				}
			}

			g.Dispose();

			return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());
		}

		internal static void UpdateCenter(string layer, List<Segment> superPixels)
		{
			foreach (var superPixel in superPixels)
			{
				var averData = GetAverageData(superPixel, layer);
				superPixel.Centroid.Pixels[layer] = new Pixel(averData);
			}
		}

		internal static void AddCenter(string layer, List<Segment> superPixels)
		{
			foreach (var superPixel in superPixels)
			{
				var averData = GetAverageData(superPixel, layer);
				superPixel.Data.ForEach(d =>
				{
					d.Pixels.AddPixel(layer, new Pixel(averData));
				});
				superPixel.Centroid.Pixels.AddPixel(layer, new Pixel(averData));
			}
		}
		private static List<float> GetGandomData(uint length)
		{
			var c = new RNGCryptoServiceProvider();
			var randomNumber = new byte[length];
			c.GetBytes(randomNumber);

			return randomNumber.Select(b => (float)b).ToList();
		}

		private static float[] GetAverageData(Segment segment, string layer)
		{
			return GetAverageData(new List<Segment> {segment}, layer);
		}

		private static float[] GetAverageData(IReadOnlyCollection<Segment> segments, string layer)
		{
			var averData = new float[4];

			foreach (var segment in segments)
			{
				segment.Data.ForEach(d =>
				{
					for (int i = 0; i < averData.Length; i++)
					{
						averData[i] += d.Pixels[layer].Data[i];
					}
				});
			}
			var count = segments.Sum(s => s.Data.Count);
			for (int i = 0; i < averData.Length; i++)
			{
				averData[i] /= count;
			}

			return averData;
		}

		private static Color GetColorFromArgb(IReadOnlyList<float> data)
		{
			return Color.FromArgb((byte) data[3], (byte) data[2], (byte) data[1], (byte) data[0]);
		}
	}
}