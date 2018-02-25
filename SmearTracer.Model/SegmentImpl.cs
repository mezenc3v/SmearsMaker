using SmearsMaker.Common.BaseTypes;

namespace SmearTracer.Model
{
	public class SegmentImpl : Segment
	{
		public SegmentImpl()
		{
			
		}

		public SegmentImpl(Point centroid)
		{
			Centroid = centroid;
		}
	}
}