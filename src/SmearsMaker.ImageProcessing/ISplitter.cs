using System.Collections.Generic;
using SmearsMaker.ImageProcessing.Segmenting;

namespace SmearsMaker.ImageProcessing
{
	public interface ISplitter
	{
		List<Segment> Splitting(Segment segment);
	}
}