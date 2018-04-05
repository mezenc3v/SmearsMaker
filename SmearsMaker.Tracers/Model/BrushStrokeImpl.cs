using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.SmearsFormation;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Model
{
	public class BrushStrokeImpl : BrushStroke
	{
		public BrushStrokeImpl()
		{
		}
		public BrushStrokeImpl(List<BaseShape> baseObjects) : base(baseObjects)
		{
		}
		public override int Width { get; }
		public override int Length
		{
			get
			{
				var length = 0d;
				for (int i = 1; i < Objects.Count; i++)
				{
					length += Utils.SqrtDistance(Objects[i - 1].GetCenter(), Objects[i].GetCenter());
				}
				return (int)length;
			}
		}

		public override double GetDistance(BrushStroke stroke)
		{
			return new List<double>
			{
				Utils.SqrtDistance(Head, stroke.Head),
				Utils.SqrtDistance(Head, stroke.Tail),
				Utils.SqrtDistance(Tail, stroke.Tail),
				Utils.SqrtDistance(Tail, stroke.Head)
			}.Min();
		}

		public override Pixel AverageData => ComputeAverageData();

		private Pixel ComputeAverageData()
		{
			var centers = Objects.Select(o => o.GetCenter(Layers.Original)).ToList();
			var length = centers.First().Data.Length;
			var averageData = new float[length];
			foreach (var center in centers)
			{
				for (int i = 0; i < length; i++)
				{
					averageData[i] += center.Data[i];
				}
			}

			for (int i = 0; i < length; i++)
			{
				averageData[i] /= centers.Count;
			}

			return Pixel.CreateInstance(averageData);
		}
	}
}