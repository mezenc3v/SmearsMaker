using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;

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
	}
}