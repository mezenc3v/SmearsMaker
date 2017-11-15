using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NLog;
using SmearsMaker.Model;

namespace SmearsMaker.Logic
{
	public static class Helper
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		
		public static void Concat(List<Smear> smears, List<Point> points)
		{
			foreach (var point in points)
			{
				var minDist = Distance(smears.First().BrushStroke.Objects.First().Centroid, point);
				var nearestObj = smears.First().BrushStroke.Objects.First();
				foreach (var smear in smears)
				{
					foreach (var obj in smear.BrushStroke.Objects)
					{
						var dist = Distance(obj.Centroid, point);
						if (dist < minDist)
						{
							nearestObj = obj;
							minDist = dist;
						}
					}
				}
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
			return Math.Sqrt(Math.Pow((first.Position.X - second.Position.X),2) + Math.Pow((first.Position.Y - second.Position.Y), 2));
		}
	}
}