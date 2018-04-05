using System.ComponentModel;
using System.Linq;

namespace SmearsMaker.Common.BaseTypes
{
	public abstract class BaseShape
	{
		public PointCollection Points { get; }

		private System.Windows.Point? _centerPoint;

		private Point _center;

		protected BaseShape()
		{
			Points = new PointCollection();
			Points.PropertyChanged += PropertyChanged;
		}
		public System.Windows.Point GetCenter()
		{
			if (_centerPoint == null)
			{
				_centerPoint = ComputeCenterWinPoint();
			}

			return _centerPoint.Value;
		}

		public Point GetCenterPoint()
		{
			return _center ?? (_center = ComputeCenterPoint());
		}

		public Pixel GetCenter(string layer)
		{
			return GetCenterPoint().Pixels[layer];
		}

		private System.Windows.Point ComputeCenterWinPoint()
		{
			var x = 0d;
			var y = 0d;
			foreach (var point in Points)
			{
				x += point.Position.X;
				y += point.Position.Y;
			}

			x /= Points.Count;
			y /= Points.Count;

			return new System.Windows.Point(x, y);
		}

		private Pixel ComputeCenter(string layer)
		{
			var dataArr = new float[Points.First().Pixels[layer].Length];
			foreach (var point in Points)
			{
				var currData = point.Pixels[layer].Data;
				for (int i = 0; i < dataArr.Length; i++)
				{
					dataArr[i] += currData[i];
				}
			}

			for (int i = 0; i < dataArr.Length; i++)
			{
				dataArr[i] /= Points.Count;
			}

			return new Pixel(dataArr);
		}

		private Point ComputeCenterPoint()
		{
			var point = new Point(GetCenter());
			foreach (var layer in Points.Layers)
			{
				var pixel = ComputeCenter(layer);
				point.Pixels.AddPixel(layer, pixel);
			}

			return point;
		}

		private void PropertyChanged(object obj, PropertyChangedEventArgs args)
		{
			ClearCache();
		}

		private void ClearCache()
		{
			_center = null;
			_centerPoint = null;
		}
	}
}