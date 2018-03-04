using System;
using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.Logic
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
	}
}