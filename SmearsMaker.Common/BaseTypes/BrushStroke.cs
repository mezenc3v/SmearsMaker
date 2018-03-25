using System.Collections.Generic;
using System.Linq;

namespace SmearsMaker.Common.BaseTypes
{
	public abstract class BrushStroke
	{
		public List<Segment> Objects { get; }
		public virtual System.Windows.Point Head => Objects.First().Centroid.Position;
		public virtual System.Windows.Point Tail => Objects.Last().Centroid.Position;
		public abstract Pixel AverageData { get; }
		
		public abstract int Width { get; }

		public abstract int Length { get; }

		public abstract double GetDistance(BrushStroke stroke);

		protected BrushStroke()
		{
			Objects = new List<Segment>();
		}

		protected BrushStroke(List<Segment> baseObjects)
		{
			Objects = baseObjects;
		}
	}
}