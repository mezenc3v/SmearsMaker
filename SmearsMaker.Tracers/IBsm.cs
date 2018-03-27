using System.Collections.Generic;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.SmearsFormation;

namespace SmearsMaker.Tracers
{
	public interface IBsm
	{
		List<BrushStroke> Execute(List<Segment> objs);
	}
}