using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.SmearTracer.Logic
{
	public static class Merger
	{
		internal static void MergePointsWithSmears(List<Smear> smears, List<Point> points)
		{
			foreach (var point in points)
			{
				var minDist = Utils.ManhattanDistance(smears.First().BrushStroke.Objects.First().Centroid, point);
				var nearestObj = smears.First().BrushStroke.Objects.First();
				Parallel.ForEach(smears, smear =>
				{
					foreach (var obj in smear.BrushStroke.Objects)
					{
						var dist = Utils.ManhattanDistance(obj.Centroid, point);
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
	}
}