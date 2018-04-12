using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.StrokesFormation;
using SmearsMaker.Tracers.Helpers;
using SmearsMaker.Tracers.Model;

namespace SmearsMaker.Tracers.Logic
{
	public class PairStrokesBuilder : IStrokesBuilder
	{
		private readonly double _maxDistance;

		public PairStrokesBuilder(double width)
		{
			_maxDistance = width;
		}

		public List<BrushStroke> Execute(List<Segment> objs)
		{
			var pairs = Pairing(objs);
			var brushStrokes = Combining(pairs);
			return brushStrokes;
		}

		private List<BrushStroke> Combining(List<BrushStroke> sequences)
		{
			bool distanceCheck;

			do
			{
				distanceCheck = false;
				if (sequences.Count > 1)
				{
					for (int i = 0; i < sequences.Count && sequences.Count > 1; i++)
					{
						var index = NearestPart(sequences, sequences[i]);

						if (sequences[i].GetDistance(sequences[index]) < _maxDistance)
						{
							distanceCheck = true;
							var combinedSequence = Combine(sequences[i], sequences[index]);
							var segmentToBeDeleted = sequences[index];
							sequences.RemoveAt(i);
							sequences.Remove(segmentToBeDeleted);
							sequences.Add(combinedSequence);
						}
					}
				}
			} while (distanceCheck);

			return sequences;
		}

		private static BrushStroke Combine(BrushStroke first, BrushStroke second)
		{
			var newSequence = new BrushStrokeImpl();

			var a = Utils.SqrtDistance(first.Objects.First().GetCenter(), second.Objects.First().GetCenter());
			var b = Utils.SqrtDistance(first.Objects.Last().GetCenter(), second.Objects.Last().GetCenter());
			var c = Utils.SqrtDistance(first.Objects.First().GetCenter(), second.Objects.Last().GetCenter());
			var d = Utils.SqrtDistance(first.Objects.Last().GetCenter(), second.Objects.First().GetCenter());

			if (a < b)
			{
				if (a < c)
				{
					if (a < d)
					{
						first.Objects.Reverse();
						newSequence.Objects.AddRange(first.Objects);
						newSequence.Objects.AddRange(second.Objects);
					}
					else
					{
						newSequence.Objects.AddRange(first.Objects);
						newSequence.Objects.AddRange(second.Objects);
					}
				}
				else
				{
					if (c < d)
					{
						newSequence.Objects.AddRange(second.Objects);
						newSequence.Objects.AddRange(first.Objects);
					}
					else
					{
						newSequence.Objects.AddRange(first.Objects);
						newSequence.Objects.AddRange(second.Objects);
					}
				}
			}
			else
			{
				if (b < c)
				{
					if (b < d)
					{
						second.Objects.Reverse();
						newSequence.Objects.AddRange(first.Objects);
						newSequence.Objects.AddRange(second.Objects);
					}
					else
					{
						newSequence.Objects.AddRange(first.Objects);
						newSequence.Objects.AddRange(second.Objects);
					}
				}
				else
				{
					if (c < d)
					{
						newSequence.Objects.AddRange(second.Objects);
						newSequence.Objects.AddRange(first.Objects);
					}
					else
					{
						newSequence.Objects.AddRange(first.Objects);
						newSequence.Objects.AddRange(second.Objects);
					}
				}
			}

			return newSequence;
		}

		private static int NearestPart(IList<BrushStroke> sequences, BrushStroke seq)
		{
			var headPosition = seq.Objects.First().GetCenter();
			var tailPosition = seq.Objects.Last().GetCenter();

			double minDistance;
			int index;

			if (sequences.First() != seq)
			{
				minDistance = Utils.SqrtDistance(sequences[0].Objects.First().GetCenter(), headPosition);
				index = 0;
			}
			else
			{
				minDistance = Utils.SqrtDistance(sequences[1].Objects.First().GetCenter(), headPosition);
				index = 1;
			}

			foreach (var sequence in sequences.Where(s => s != seq))
			{
				var hh = Utils.SqrtDistance(sequence.Objects.First().GetCenter(), headPosition);
				var ht = Utils.SqrtDistance(sequence.Objects.First().GetCenter(), tailPosition);
				var tt = Utils.SqrtDistance(sequence.Objects.Last().GetCenter(), tailPosition);
				var th = Utils.SqrtDistance(sequence.Objects.Last().GetCenter(), headPosition);

				if (hh > 0 && minDistance > hh)
				{
					index = sequences.IndexOf(sequence);
					minDistance = hh;
				}

				if (ht > 0 && minDistance > ht)
				{
					index = sequences.IndexOf(sequence);
					minDistance = ht;
				}

				if (tt > 0 && minDistance > tt)
				{
					index = sequences.IndexOf(sequence);
					minDistance = tt;
				}

				if (th > 0 && minDistance > th)
				{
					index = sequences.IndexOf(sequence);
					minDistance = th;
				}
			}

			return index;
		}

		private static int FindNearestSequence(IReadOnlyList<BrushStroke> sequences, Point point)
		{
			var index = 0;
			var minDistance = Utils.SqrtDistance(sequences.First().Objects.First().GetCenter(), point.Position);

			for (int i = 0; i < sequences.Count; i++)
			{
				var distanceHead = Utils.SqrtDistance(sequences[i].Objects.First().GetCenter(), point.Position);
				var distanceTail = Utils.SqrtDistance(sequences[i].Objects.Last().GetCenter(), point.Position);

				if (minDistance > distanceHead)
				{
					minDistance = distanceHead;
					index = i;
				}

				if (minDistance > distanceTail)
				{
					minDistance = distanceTail;
					index = i;
				}
			}

			return index;
		}

		private static BrushStroke Combine(BrushStroke first, BaseShape second)
		{
			var newSequence = new BrushStrokeImpl();

			var distanceHead = Utils.SqrtDistance(first.Objects.First().GetCenter(), second.GetCenter());
			var distanceTail = Utils.SqrtDistance(first.Objects.Last().GetCenter(), second.GetCenter());

			if (distanceHead > distanceTail)
			{
				newSequence.Objects.AddRange(first.Objects);
				newSequence.Objects.Add(second);
			}
			else
			{
				newSequence.Objects.Add(second);
				newSequence.Objects.AddRange(first.Objects);
			}

			return newSequence;
		}

		private static System.Windows.Point GetCenter(IReadOnlyCollection<Segment> objs)
		{
			var x = 0d;
			var y = 0d;
			foreach (var obj in objs)
			{
				x += obj.GetCenter().X;
				y += obj.GetCenter().Y;
			}

			return new System.Windows.Point(x / objs.Count, y / objs.Count);
		}
		private List<BrushStroke> Pairing(IReadOnlyCollection<Segment> objs)
		{
			var brushStrokes = new List<BrushStroke>();
			var points = new List<Segment>();
			points.AddRange(objs);

			if (objs.Count > 1)
			{
				var center = GetCenter(objs);

				var startPoint = objs.OrderBy(p => Utils.SqrtDistance(center, p.GetCenter())).First();

				while (points.Count > 0)
				{
					if (points.Count > 1)
					{
						var list = new List<BaseShape>();
						var main = points.OrderBy(p => Utils.SqrtDistance(startPoint.GetCenter(), p.GetCenter())).Last();

						list.Add(main);
						points.Remove(main);

						var next = points.OrderBy(p => Utils.SqrtDistance(main.GetCenter(), p.GetCenter())).First();

						if (Utils.SqrtDistance(next.GetCenter(), main.GetCenter()) < _maxDistance)
						{
							list.Add(next);
							points.Remove(next);
							brushStrokes.Add(new BrushStrokeImpl(list));
						}
						else
						{
							var index = FindNearestSequence(brushStrokes, list.First().GetCenterPoint());
							var newSequence = Combine(brushStrokes[index], list.First());

							brushStrokes.RemoveAt(index);
							brushStrokes.Add(newSequence);
						}
					}
					else
					{
						var index = FindNearestSequence(brushStrokes, points.First().GetCenterPoint());
						var newSequence = Combine(brushStrokes[index], points.First());

						brushStrokes.RemoveAt(index);
						brushStrokes.Add(newSequence);
						points = new List<Segment>();
					}
				}
			}
			else
			{
				var brushStroke = new BrushStrokeImpl();
				brushStroke.Objects.Add(objs.First());
				brushStrokes.Add(brushStroke);
			}

			return brushStrokes;
		}
	}
}