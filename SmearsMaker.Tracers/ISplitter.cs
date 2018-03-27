using System.Collections.Generic;
using SmearsMaker.ImageProcessing.Segmenting;

namespace SmearsMaker.Tracers
{
	public interface ISplitter
	{
		List<Segment> Splitting(Segment segment);
	}
}