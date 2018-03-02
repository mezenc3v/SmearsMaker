using System;
using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace GradientTracer.Logic
{
	public class TurboPixelSplitter
	{
		private readonly int _size;
		private readonly double _tolerance;
		public TurboPixelSplitter(int size, double tolerance)
		{
			_size = size;
			_tolerance = tolerance;
		}

		public List<Segment> Splitting(Segment segment)
		{
			return null;
		}

		private static double Distance(System.Windows.Point first, System.Windows.Point second)
		{
			var sum = Math.Pow(first.X - second.X, 2);
			sum += Math.Pow(first.Y - second.Y, 2);
			return Math.Sqrt(sum);
		}
	}
}