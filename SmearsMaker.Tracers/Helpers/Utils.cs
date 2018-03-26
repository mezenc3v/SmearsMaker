using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;
using Point = System.Windows.Point;

namespace SmearsMaker.Tracers.Helpers
{
	public static partial class Utils
	{
		internal static void UpdateCenter(string layer, List<Segment> superPixels)
		{
			foreach (var superPixel in superPixels)
			{
				var averData = GetAverageData(superPixel, layer);
				superPixel.Centroid.Pixels[layer] = new Pixel(averData);
				var center = new Point();
				foreach (var point in superPixel.Data)
				{
					center.X += point.Position.X;
					center.Y += point.Position.Y;
				}

				center.X /= superPixel.Data.Count;
				center.Y /= superPixel.Data.Count;

				superPixel.Centroid.Position = center;
			}
		}

		internal static void AddCenter(string layer, List<Segment> segments)
		{
			foreach (var segment in segments)
			{
				AddCenter(layer, segment);
			}
		}

		internal static void AddCenter(string layer, Segment segment)
		{
			var averData = GetAverageData(segment, layer);
			segment.Data.ForEach(d =>
			{
				d.Pixels.AddPixel(layer, new Pixel(averData));
			});
			segment.Centroid.Pixels.AddPixel(layer, new Pixel(averData));
		}

		internal static float[] GetAverageData(IReadOnlyCollection<Segment> segments, string layer)
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

		private static float[] GetAverageData(Segment segment, string layer)
		{
			return GetAverageData(new List<Segment> { segment }, layer);
		}

		internal static List<float> GetGandomData(uint length)
		{
			var c = new RNGCryptoServiceProvider();
			var randomNumber = new byte[length];
			c.GetBytes(randomNumber);

			return randomNumber.Select(b => (float)b).ToList();
		}
	}
}