using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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

		public static BitmapSource ToBitmapSource(DrawingImage source, int width, int heigth)
		{
			var drawingVisual = new DrawingVisual();
			var drawingContext = drawingVisual.RenderOpen();
			var bmp = new RenderTargetBitmap(width, heigth, 96, 96, PixelFormats.Pbgra32);
			drawingContext.DrawImage(source, new Rect(new System.Windows.Point(0, 0), new Size(width, heigth)));
			drawingContext.Close();
			bmp.Render(drawingVisual);
			return bmp;
		}

		internal static BitmapSource PaintStrokes(BitmapSource source, IEnumerable<BrushStroke> strokes, float width)
		{
			var geometries = new DrawingGroup();
			var radius = 1;
			foreach (var stroke in strokes.OrderByDescending(s => s.Objects.Count))
			{
				var center = Utils.GetAverageData(stroke.Objects, Layers.Original);
				var brush = new SolidColorBrush(GetColorFromArgb(center));
				var pen = new Pen(brush, width);

				var points = stroke.Objects.Select(point => new System.Windows.Point((int)point.Centroid.Position.X, (int)point.Centroid.Position.Y)).ToArray();

				var lastPoint = new EllipseGeometry(points.Last(), radius, radius);
				geometries.Children.Add(new GeometryDrawing(brush, pen, lastPoint));
				if (points.Length > 1)
				{
					for (int i = 0; i < points.Length - 1; i++)
					{
						var line = new LineGeometry(points[i], points[i + 1]);
						var head = new EllipseGeometry(points[i], radius, radius);
						var tail = new EllipseGeometry(points[i + 1], radius, radius);
						geometries.Children.Add(new GeometryDrawing(brush, pen, head));
						geometries.Children.Add(new GeometryDrawing(brush, pen, tail));
						geometries.Children.Add(new GeometryDrawing(brush, pen, line));
					}
				}
			}
			return ToBitmapSource(new DrawingImage(geometries), source.PixelWidth, source.PixelHeight);
		}

		private static Color GetColorFromArgb(IReadOnlyList<float> data)
		{
			return Color.FromArgb((byte)data[3], (byte)data[2], (byte)data[1], (byte)data[0]);
		}
	}
}