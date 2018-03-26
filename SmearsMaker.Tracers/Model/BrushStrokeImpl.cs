using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.SmearsFormation;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Model
{
	public class BrushStrokeImpl : BrushStroke
	{
		public BrushStrokeImpl()
		{
		}
		public BrushStrokeImpl(List<Segment> baseObjects) : base(baseObjects)
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
					length += Utils.SqrtDistance(Objects[i - 1].Centroid.Position, Objects[i].Centroid.Position);
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

		public override Pixel AverageData
		{
			get
			{
				var centers = Objects.Select(o => o.Centroid).ToList();
				var length = centers.First().Pixels[Layers.Original].Data.Length;
				var averageData = new float[length];
				foreach (var center in centers)
				{
					for (int i = 0; i < length; i++)
					{
						averageData[i] += center.Pixels[Layers.Original].Data[i];
					}
				}

				for (int i = 0; i < length; i++)
				{
					averageData[i] /= centers.Count;
				}

				return new Pixel(averageData);
			}
		}
	}
}