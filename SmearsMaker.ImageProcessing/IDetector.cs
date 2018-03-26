using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing
{
	public interface IDetector
	{
		List<Point> Compute(List<Point> points);
	}
}