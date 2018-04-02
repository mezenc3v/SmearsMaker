using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;

namespace SmearsMaker.Tracers.Extentions
{
	public static class SegmentExtentions
	{
		public static bool IsSameColor(this Segment firstSegment, Segment secondSegment, double tolerance, string layer)
		{
			var distance = Math.Abs(firstSegment.Centroid.Pixels[layer].Average - secondSegment.Centroid.Pixels[layer].Average);
			return distance < tolerance;
		}

		public static Segment FindNearest(this Segment obj, IEnumerable<Segment> objs, string layer)
		{
			var data = obj.Centroid.Pixels[layer].Data[0];
			var result = objs.OrderBy(p => Math.Abs(data - p.Centroid.Pixels[layer].Data[0]));
			return result.First();
		}

		public static float[] GetAverageData(this Segment seg, string layer)
		{
			var length = seg.Data.First().Pixels[layer].Length;
			var averData = new float[length];

			foreach (var point in seg.Data)
			{
				for (int i = 0; i < length; i++)
				{
					averData[i] += point.Pixels[layer].Data[i];
				}
			}

			for (int i = 0; i < length; i++)
			{
				averData[i] /= seg.Data.Count;
			}

			return averData;
		}
	}
}