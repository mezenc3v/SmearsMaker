using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.StrokesFormation;

namespace SmearsMaker.ImageProcessing
{
	public interface IStrokesBuilder
	{
		List<BrushStroke> Execute(List<BaseShape> objs);
	}
}