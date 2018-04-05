using System;
using System.Linq;

namespace SmearsMaker.Common.BaseTypes
{
	public class Pixel
	{
		public static Pixel CreateInstance(float[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}
			return new Pixel(data);
		}

		public float Sum => Data.Sum();
		public float Average => Data.Average();
		public float[] Data { get; set; }
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

		internal Pixel()
		{
			Data = new float[3];
		}
		internal Pixel(float[] data)
		{
			Data = (float[])data?.Clone() ?? throw new ArgumentNullException(nameof(data));
		}
		internal Pixel(Pixel pixel)
		{
			if (pixel == null)
			{
				throw new ArgumentNullException(nameof(pixel));
			}
			var data = pixel.Data;
			Data = (float[])data?.Clone();
		}
		public double Distance(float[] data)
		{
			var dist = 0d;
			for (int i = 0; i < Data.Length; i++)
			{
				dist += Distance(Data[i], data[i]);
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
