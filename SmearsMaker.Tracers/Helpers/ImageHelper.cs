using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace SmearsMaker.Tracers.Helpers
{
	internal static class ImageHelper
	{
		internal static BitmapSource CreateRandomImage(IEnumerable<Segment> objects, string layer, ImageModel model)
		{
			var data = new List<Point>();
			foreach (var obj in objects)
			{
				var rand = Utils.GetGandomData(3).ToArray();
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

		internal static BitmapSource CreateImageFromStrokes(IEnumerable<BrushStroke> strokes, string layer, ImageModel model)
		{
			var data = new List<Point>();
			foreach (var stroke in strokes)
			{
				var averageData = stroke.AverageData;
				var objects = stroke.Objects;
				foreach (var o in objects)
				{
					foreach (var point in o.Data)
					{
						point.Pixels[layer] = averageData;
					}
					data.AddRange(o.Data);
				}
			}
			return model.ConvertToBitmapSource(data, layer);
		}

		internal static BitmapSource PaintStrokes(BitmapSource source, IEnumerable<BrushStroke> strokes, float width)
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
				var center = Utils.GetAverageData(stroke.Objects, Layers.Original);
				var color = GetColorFromArgb(center);

				var brush = new SolidBrush(color);
				var pen = new Pen(color)
				{
					Width = width
				};

				var pointsF = stroke.Objects.Select(point => new PointF((float)point.Centroid.Position.X, (float)point.Centroid.Position.Y)).ToArray();

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

		private static Color GetColorFromArgb(IReadOnlyList<float> data)
		{
			return Color.FromArgb((byte)data[3], (byte)data[2], (byte)data[1], (byte)data[0]);
		}
	}
}