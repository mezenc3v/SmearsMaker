using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace SmearTracer.Model
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
					length += Distance(Objects[i - 1].Centroid.Position, Objects[i].Centroid.Position);
				}
				return (int)length;
			}
		}

		public override float[] AverageData
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

				return averageData;
			}
		}

		private static double Distance(System.Windows.Point first, System.Windows.Point second)
		{
			var sum = Math.Pow(first.X - second.X, 2);
			sum += Math.Pow(first.Y - second.Y, 2);
			return Math.Sqrt(sum);
		}
	}
}