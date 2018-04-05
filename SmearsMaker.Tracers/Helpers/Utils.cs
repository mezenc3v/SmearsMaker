using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.Helpers
{
	public static partial class Utils
	{
		internal static float[] GetAverageData(IReadOnlyCollection<BaseShape> segments, string layer)
		{
			var averData = new float[4];

			foreach (var segment in segments)
			{
				segment.Points.ForEach(d =>
				{
					for (int i = 0; i < averData.Length; i++)
					{
						averData[i] += d.Pixels[layer].Data[i];
					}
				});
			}
			var count = segments.Sum(s => s.Points.Count);
			for (int i = 0; i < averData.Length; i++)
			{
				averData[i] /= count;
			}

			return averData;
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