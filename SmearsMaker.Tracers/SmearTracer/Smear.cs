using SmearsMaker.ImageProcessing.Clustering;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.StrokesFormation;

namespace SmearsMaker.Tracers.SmearTracer
{
	public class Smear
	{
		public BrushStroke BrushStroke { get; }
		public Segment Segment { get; set; }
		public Cluster Cluster { get; set; }

		public Smear(BrushStroke brushStroke)
		{
			BrushStroke = brushStroke;
		}
	}
}