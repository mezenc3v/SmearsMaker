using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing.SmearsFormation;
using SmearsMaker.Tracers.Extentions;
using PointCollection = SmearsMaker.Common.BaseTypes.PointCollection;

namespace SmearsMaker.Tracers.Helpers
{
	internal static class ImageHelper
	{
		internal static BitmapSource CreateRandomImage(IEnumerable<BaseShape> objects, string layer, ImageModel model)
		{
			var data = new PointCollection();
			foreach (var obj in objects)
			{
				var points = obj.Points.Clone();
				var rand = Utils.GetGandomData(3).ToArray();
				points.ForEach(d =>
				{
					d.Pixels[layer].Data = rand;
				});
				data.AddRange(points);
			}
			return model.ConvertToBitmapSource(data, layer);
		}
		internal static BitmapSource CreateImage(IEnumerable<BaseShape> objects, string layer, ImageModel model)
		{
			var data = new PointCollection();
			foreach (var obj in objects)
			{
				var averData = obj.GetAverageData(layer);
				var points = obj.Points.Clone();
				foreach (var point in points)
				{
					for (int i = 0; i < point.Pixels[layer].Length; i++)
					{
						point.Pixels[layer].Data[i] = averData[i];
					}
				}
				data.AddRange(points);
			}
			return model.ConvertToBitmapSource(data, layer);
		}

		internal static BitmapSource CreateImageFromStrokes(IEnumerable<BrushStroke> strokes, string layer, ImageModel model)
		{
			var data = new PointCollection();
			foreach (var stroke in strokes)
			{
				var averageData = stroke.AverageData;
				foreach (var obj in stroke.Objects)
				{
					var points = obj.Points.Clone();
					foreach (var point in points)
					{
						point.Pixels[layer] = averageData;
					}
					data.AddRange(points);
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
				var center = Utils.GetAverageData(stroke.Objects, Layers.Filtered);
				var brush = new SolidColorBrush(GetColorFromArgb(center));
				var linePen = new Pen(brush, width);
				var circlePen = new Pen(brush, width - 1);

				var points = stroke.Objects.Select(point => new System.Windows.Point((int)point.GetCenter().X, (int)point.GetCenter().Y)).ToArray();

				var lastPoint = new EllipseGeometry(points.Last(), radius, radius);
				geometries.Children.Add(new GeometryDrawing(brush, circlePen, lastPoint));
				if (points.Length > 1)
				{
					for (int i = 0; i < points.Length - 1; i++)
					{
						var line = new LineGeometry(points[i], points[i + 1]);
						var head = new EllipseGeometry(points[i], radius, radius);
						var tail = new EllipseGeometry(points[i + 1], radius, radius);
						geometries.Children.Add(new GeometryDrawing(brush, circlePen, head));
						geometries.Children.Add(new GeometryDrawing(brush, circlePen, tail));
						geometries.Children.Add(new GeometryDrawing(brush, linePen, line));
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