using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers
{
	public interface ISplitter
	{
		List<Segment> Splitting(Segment segment, int length);
	}
}