using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using GradientTracer.Model;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace GradientTracer.Analyzer
{
	internal static class GTHelper
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

			foreach (var smear in strokes.OrderByDescending(s => s.Objects.Count))
			{
				var center = smear.Objects.OrderBy(p => p.Centroid.Pixels[GtLayers.Original].Sum).ToList()[smear.Objects.Count / 2].Centroid;

				var pen = new Pen(Color.FromArgb((byte)center.Pixels[GtLayers.Original].Data[3], (byte)center.Pixels[GtLayers.Original].Data[2],
					(byte)center.Pixels[GtLayers.Original].Data[1], (byte)center.Pixels[GtLayers.Original].Data[0]));

				var brush = new SolidBrush(Color.FromArgb((byte)center.Pixels[GtLayers.Original].Data[3], (byte)center.Pixels[GtLayers.Original].Data[2],
					(byte)center.Pixels[GtLayers.Original].Data[1], (byte)center.Pixels[GtLayers.Original].Data[0]));
				//var random = GetGandomData(4);
				//var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb((byte)random[0], (byte)random[1],
				//	(byte)random[2], (byte)random[3]));

				//var brush = new SolidBrush(System.Drawing.Color.FromArgb((byte)random[0], (byte)random[1],
				//	(byte)random[2], (byte)random[3]));

				var pointsF = smear.Objects.Select(point => new PointF((int)point.Centroid.Position.X, (int)point.Centroid.Position.Y)).ToArray();
				pen.Width = Width;
				if (pointsF.Length > 1)
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
					g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y, pen.Width, pen.Width);
					g.DrawLines(pen, pointsF);
				}
				else
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
				}
			}

			g.Dispose();

			var bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			return bmp;
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

		private static float[] GetAverageData(Segment superPixel, string layer)
		{
			var averData = new float[4];
			superPixel.Data.ForEach(d =>
			{
				for (int i = 0; i < averData.Length; i++)
				{
					averData[i] += d.Pixels[layer].Data[i];
				}
			});

			for (int i = 0; i < averData.Length; i++)
			{
				averData[i] /= superPixel.Data.Count;
			}

			return averData;
		}
	}
}