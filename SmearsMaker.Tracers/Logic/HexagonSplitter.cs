using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.Logic
{
	public class HexagonSplitter : Splitter
	{
		public HexagonSplitter(int size) : base(size)
		{
		}

		protected override Point GetCentroid(Segment superPixel)
		{
			throw new System.NotImplementedException();
		}

		protected override int NearestCentroid(Point pixel, IReadOnlyList<Segment> superPixels)
		{
			throw new System.NotImplementedException();
		}

		protected override IEnumerable<System.Windows.Point> PlacementCenters(double diameter, Segment segment)
		{
			throw new System.NotImplementedException();
		}
	}
}