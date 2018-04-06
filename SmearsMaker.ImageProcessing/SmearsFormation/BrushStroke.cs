using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing.SmearsFormation
{
	public abstract class BrushStroke
	{
		public Guid Id { get; }
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
			Id = Guid.NewGuid();
		}

		protected BrushStroke(List<BaseShape> baseObjects)
		{
			Objects = baseObjects;
			Id = Guid.NewGuid();
		}
	}
}