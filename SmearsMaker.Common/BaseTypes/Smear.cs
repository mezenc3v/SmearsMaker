namespace SmearsMaker.Common.BaseTypes
{
	public class Smear
	{
		public BrushStroke BrushStroke { get; }
		public Segment Segment { get; set; }
		public Cluster Cluster { get; set; }

		public Smear(BrushStroke brushStroke)
		{
			BrushStroke = brushStroke;
		}
	}
}