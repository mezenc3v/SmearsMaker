using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers
{
	public interface IBsm
	{
		List<BrushStroke> Execute(List<Segment> objs, double width, float toleranceFirst, float toleranceSecond);
	}
}