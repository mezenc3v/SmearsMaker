using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SmearsMaker.Common.BaseTypes;

namespace SmearTracer.Analyzer
{
	public static class Helper
	{
		public static void Concat(List<Smear> smears, List<Point> points)
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

		public static List<float> GetGandomData(uint length)
		{
			var c = new RNGCryptoServiceProvider();
			var randomNumber = new byte[length];
			c.GetBytes(randomNumber);
			
			return randomNumber.Select(b => (float) b).ToList();
		}

		private static double Distance(Point first, Point second)
		{
			//return Math.Sqrt(Math.Pow((first.Position.X - second.Position.X),2) + Math.Pow((first.Position.Y - second.Position.Y), 2));

			double dictance = 0;

			var d = first.Position.X - second.Position.X;
			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			d = first.Position.Y - second.Position.Y;

			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			return dictance;
		}
	}
}