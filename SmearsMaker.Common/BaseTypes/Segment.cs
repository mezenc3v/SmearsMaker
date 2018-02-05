using System.Collections.Generic;

namespace SmearsMaker.Common.BaseTypes
{
	public class Segment : BaseObject
	{
		public Segment()
		{
			Data = new List<Point>();
		}
	}
}