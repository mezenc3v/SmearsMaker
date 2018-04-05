using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.SmearsFormation
{
	public abstract class BrushStroke
	{
		public List<BaseShape> Objects { get; }
		public virtual System.Windows.Point Head => Objects.First().GetCenter();
		public virtual System.Windows.Point Tail => Objects.Last().GetCenter();
		public abstract Pixel AverageData { get; }
		
		public abstract int Width { get; }

		public abstract int Length { get; }

		public abstract double GetDistance(BrushStroke stroke);

		protected BrushStroke()
		{
			Objects = new List<BaseShape>();
		}

		protected BrushStroke(List<BaseShape> baseObjects)
		{
			Objects = baseObjects;
		}
	}
}