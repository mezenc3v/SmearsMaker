using System.Collections.Generic;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.StrokesFormation;

namespace SmearsMaker.ImageProcessing
{
	public interface IStrokesBuilder
	{
		List<BrushStroke> Execute(List<Segment> objs);
	}
}