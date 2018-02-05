using System;
using System.Collections.Generic;

namespace SmearsMaker.Common.BaseTypes
{
	public class SuperPixel : BaseObject
	{
	public SuperPixel(Point point)
		{
			Centroid = point ?? throw new NullReferenceException("point");
			Data = new List<Point>();
		}

	}
}