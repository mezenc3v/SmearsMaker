using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SmearsMaker.HPGL
{
	internal class Painter
	{
		private const int Radius = 1;
		private SolidColorBrush _brush;
		private Pen _circlePen;
		private Pen _linePen;

		private readonly DrawingGroup _geometries;

		public DrawingImage Image => new DrawingImage(_geometries);

		internal Painter()
		{
			_geometries = new DrawingGroup();
		}

		internal void SetPens(byte[] data, int width)
		{
			_brush = new SolidColorBrush(Color.FromRgb(data[0], data[1], data[2]));
			_circlePen = new Pen(_brush, width - 1);
			_linePen = new Pen(_brush, width);
		}

		internal void AddPoints(List<Point> points)
		{
			if (points == null)
				throw new ArgumentNullException(nameof(points));
			if (points.Count <= 1)
				return;
			for (int i = 0; i < points.Count - 1; i++)
			{
				var line = new LineGeometry(points[i], points[i + 1]);
				var head = new EllipseGeometry(points[i], Radius, Radius);
				var tail = new EllipseGeometry(points[i + 1], Radius, Radius);
				_geometries.Children.Add(new GeometryDrawing(_brush, _circlePen, head));
				_geometries.Children.Add(new GeometryDrawing(_brush, _circlePen, tail));
				_geometries.Children.Add(new GeometryDrawing(_brush, _linePen, line));
			}
		}

	}
}
