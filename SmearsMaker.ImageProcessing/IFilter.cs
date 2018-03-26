using System.Collections.Generic;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing
{
	public interface IFilter
	{
		List<Point> Filtering(List<Point> points);
	}
}