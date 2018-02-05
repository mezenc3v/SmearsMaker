using System;
using System.Linq;

namespace SmearsMaker.Common.BaseTypes
{
	public class Pixel
	{
		public float Sum => Data.Sum();
		public float[] Data { get; }

		public int Length => Data.Length;

		public float GrayScale
		{
			get
			{
				{
					//var grayscale = (byte)(0.2126 * r + 0.7152 * g + 0.0722 * b);
					var grayscale = (float)(0.299 * Data[0] + 0.587 * Data[1] + 0.114 * Data[2]);
					return grayscale;
				};
			}
		}

		public Pixel(float[] data)
		{
			Data = (float[])data?.Clone() ?? throw new ArgumentNullException(nameof(data));
		}

		public Pixel(Pixel pixel)
		{
			if (pixel == null)
			{
				throw new ArgumentNullException(nameof(pixel));
			}
			var data = pixel.Data;
			Data = (float[])data?.Clone();
		}

		public double Distance(Pixel pixel)
		{
			//return Math.Sqrt(_colorData.Select((t, i) => Math.Pow(t - pixel._colorData[i], 2)).Sum());
			//return _colorData.Select((t, i) => Math.Abs(t - pixel._colorData[i])).Sum();
			//return _colorData.Select((t, i) => Distance(t, pixel._colorData[i])).Sum();

			var dist = 0d;
			for (int i = 0; i < Data.Length; i++)
			{
				dist += Distance(Data[i], pixel.Data[i]);
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
