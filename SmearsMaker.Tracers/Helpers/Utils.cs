using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.SmearTracer;

namespace SmearsMaker.Tracers.Helpers
{
	public static partial class Utils
	{
		internal static void Concat(List<Smear> smears, List<Point> points)
		{
			foreach (var point in points)
			{
				var minDist = Distance(smears.First().BrushStroke.Objects.First().Centroid, point);
				var nearestObj = smears.First().BrushStroke.Objects.First();
				Parallel.ForEach(smears, smear =>
				{
					foreach (var obj in smear.BrushStroke.Objects)
					{
						var dist = Distance(obj.Centroid, point);
						if (dist < minDist)
						{
							lock (nearestObj)
							{
								nearestObj = obj;
							}

							minDist = dist;
						}
					}
				});
				nearestObj.Data.Add(point);
			}
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
		internal static float[] GetAverageData(Segment segment, string layer)
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