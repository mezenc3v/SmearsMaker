using SmearsMaker.Common.BaseTypes;

namespace GradientTracer.Model
{
	public class SegmentImpl : Segment
	{
		public SegmentImpl(Point centroid)
		{
			Centroid = centroid;
		}

		public SegmentImpl(Point centroid, int width, int height)
		{
			Centroid = centroid;
			MaxX = new System.Windows.Point(width, height);
			MaxY = new System.Windows.Point(width, height);
			MinX = new System.Windows.Point(0, 0);
			MinY = new System.Windows.Point(0, 0);
		}
	}
}