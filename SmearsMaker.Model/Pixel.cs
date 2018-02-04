using System;
using System.Linq;

namespace SmearsMaker.Model
{
	public class Pixel
	{
		public float Sum => _colorData.Sum();
		public float[] Data => _colorData.Clone() as float[];
		public int Length => _colorData.Length;

		public float GrayScale
		{
			get
			{
				{
					//var grayscale = (byte)(0.2126 * r + 0.7152 * g + 0.0722 * b);
					var grayscale = (float)(0.299 * _colorData[0] + 0.587 * _colorData[1] + 0.114 * _colorData[2]);
					return grayscale;
				};
			}
		}

		private readonly float[] _colorData;

		public Pixel(float[] data)
		{
			_colorData = (float[])data?.Clone() ?? throw new ArgumentNullException(nameof(data));
		}

		public Pixel(Pixel pixel)
		{
			if (pixel == null)
			{
				throw new ArgumentNullException(nameof(pixel));
			}
			var data = pixel._colorData;
			_colorData = (float[])data?.Clone();
		}

		public double Distance(Pixel pixel)
		{
			//return Math.Sqrt(_colorData.Select((t, i) => Math.Pow(t - pixel._colorData[i], 2)).Sum());
			//return _colorData.Select((t, i) => Math.Abs(t - pixel._colorData[i])).Sum();
			//return _colorData.Select((t, i) => Distance(t, pixel._colorData[i])).Sum();

			var dist = 0d;
			for (int i = 0; i < _colorData.Length; i++)
			{
				dist += Distance(_colorData[i], pixel._colorData[i]);
			}

			return dist;
		}

		private static float Distance(float first, float second)
		{
			var distance = first - second;
			if (distance < 0)
			{
				return -distance;
			}
			return distance;
		}
	}
}
