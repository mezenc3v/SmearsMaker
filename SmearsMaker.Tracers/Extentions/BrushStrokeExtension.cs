using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.ImageProcessing.SmearsFormation;
using SmearsMaker.Tracers.Helpers;

namespace SmearsMaker.Tracers.Extentions
{
	public static class BrushStrokeExtension
	{
		public static bool IsSameColor(this BrushStroke firstSegment, BrushStroke secondSegment, double tolerance)
		{
			var distance = Math.Abs(firstSegment.AverageData.Average - secondSegment.AverageData.Average);
			return distance < tolerance;
		}

		public static BrushStroke FindFirstNearestByLayer(this BrushStroke obj, IEnumerable<BrushStroke> objs, string layer)
		{
			var gradHead = obj.Objects.First().GetCenter(layer).Data[0];
			var gradTail = obj.Objects.Last().GetCenter(layer).Data[0];
			var grads = new List<float>();

			return objs.OrderBy(p =>
			{
				grads.Add(Math.Abs(gradHead - obj.Objects.First().GetCenter(layer).Data[0]));
				grads.Add(Math.Abs(gradTail - obj.Objects.Last().GetCenter(layer).Data[0]));
				grads.Add(Math.Abs(gradHead - obj.Objects.Last().GetCenter(layer).Data[0]));
				grads.Add(Math.Abs(gradTail - obj.Objects.First().GetCenter(layer).Data[0]));
				return grads.Min();
			}).First();
		}

		public static List<BrushStroke> FindAllNearest(this BrushStroke seq, IList<BrushStroke> sequences)
		{
			var result = new List<BrushStroke>();
			var head = seq.Head;
			var tail = seq.Tail;

			double minDistance;

			if (sequences.First() != seq)
			{
				minDistance = Utils.SqrtDistance(sequences[0].Head, head);
				result.Add(sequences.First());
			}
			else
			{
				minDistance = Utils.SqrtDistance(sequences[1].Head, head);
				result.Add(sequences[1]);
			}

			foreach (var sequence in sequences.Where(s => s != seq))
			{
				new List<double>{
					Utils.SqrtDistance(sequence.Head, head),
					Utils.SqrtDistance(sequence.Head, tail),
					Utils.SqrtDistance(sequence.Tail, tail),
					Utils.SqrtDistance(sequence.Tail, head)
				}.ForEach(dist =>
				{
					if (minDistance > dist)
					{
						minDistance = dist;
						result = new List<BrushStroke> { sequence };
					}
					else if (minDistance == dist)
					{
						result.Add(sequence);
					}
				});
			}

			return result;
		}

	}
}