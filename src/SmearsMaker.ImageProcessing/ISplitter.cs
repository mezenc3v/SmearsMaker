using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;

namespace SmearsMaker.ImageProcessing
{
	public interface ISplitter
	{
		List<Segment> Splitting(BaseShape segment);
	}
}