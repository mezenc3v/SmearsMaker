using SmearsMaker.ClusterAnalysis.Kmeans;
using SmearsMaker.Concatenation;
using SmearsMaker.Segmentation.SimpleSegmentsSplitter;

namespace SmearsMaker.SmearTracer
{
	public class Smear
	{
		public BrushStroke BrushStroke { get;}
		public Segment Segment { get; set; }
		public Cluster Cluster { get; set; }

		public Smear(BrushStroke brushStroke)
		{
			BrushStroke = brushStroke;
		}
	}
}