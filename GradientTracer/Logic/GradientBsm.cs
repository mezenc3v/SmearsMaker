using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GradientTracer.Model;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace GradientTracer.Logic
{
	public class GradientBsm
	{
		private readonly Random _rnd;
		private readonly double _maxDistance;
		private readonly double _tolerance;

		public GradientBsm(double maxDistance, float toLerance)
		{
			_tolerance = toLerance;
			_maxDistance = maxDistance;
			_rnd = new Random();
		}

		public List<BrushStroke> Execute(List<Segment> objs)
		{
			var strokesFromGroups = new List<BrushStroke>();
			var groups = SplitSegmentsByColor(objs);
			Parallel.ForEach(groups, group =>
			{
				var pairs = Pairing(group);

				var brushStrokes = pairs.Count > 1 ? Combining(pairs) : pairs;

				lock (strokesFromGroups)
				{
					strokesFromGroups.AddRange(brushStrokes);
				}
			});

			return strokesFromGroups;
		}

		private IEnumerable<IEnumerable<Segment>> SplitSegmentsByColor(IEnumerable<Segment> objs)
		{
			var groups = new List<List<Segment>>();
			var segments = new List<Segment>();
			segments.AddRange(objs);

			while (segments.Count > 0)
			{
				var segment = segments[_rnd.Next(0, segments.Count)];
				var searchableSegments = segments.FindAll(s => IsSameColor(s, segment));
				var group = new List<Segment>();
				group.AddRange(searchableSegments);
				group.Add(segment);
				segments.RemoveAll(p => searchableSegments.Contains(p));
				segments.Remove(segment);
				groups.Add(group);
			}
			return groups;
		}

		private IList<BrushStroke> Pairing(IEnumerable<Segment> objs)
		{
			var brushStrokes = new List<BrushStroke>();
			var points = new List<Segment>();
			points.AddRange(objs);

			while (points.Count > 0)
			{
				var list = new List<Segment>();
				var main = points[_rnd.Next(0, points.Count)];

				list.Add(main);
				points.Remove(main);

				var nearestPoints = points.FindAll(p => GetDistance(p.Centroid.Position, main.Centroid.Position) < _maxDistance);
				if (nearestPoints.Any())
				{
					var next = FindByGradient(main, nearestPoints);
					list.Add(next);
					points.Remove(next);
				}
				brushStrokes.Add(new BrushStrokeImpl(list));
			}

			return brushStrokes;
		}

		private IEnumerable<BrushStroke> Combining(IList<BrushStroke> sequences)
		{
			bool segmentsAreMerged;

			do
			{
				segmentsAreMerged = false;
				for (int i = 0; i < sequences.Count; i++)
				{
					var index = NearestPart(sequences, sequences[i]);
					var distance = GetDistance(sequences[index], sequences[i]);
					//var index1 = FindByGradient(sequences[i], trueSequenses);

					if (distance < _maxDistance)
					{
						var combinedSequence = Combine(sequences[index], sequences[i]);
						var segmentToBeDeleted = sequences[index];
						sequences.RemoveAt(i);
						sequences.Remove(segmentToBeDeleted);
						sequences.Add(combinedSequence);
						segmentsAreMerged = true;
					}
				}
			} while (segmentsAreMerged && sequences.Count > 1);

			return sequences;
		}

		private static BrushStroke Combine(BrushStroke first, BrushStroke second)
		{
			var newSequence = new BrushStrokeImpl();

			var a = GetDistance(first.Head, second.Head);
			var b = GetDistance(first.Tail, second.Tail);
			var c = GetDistance(first.Head, second.Tail);
			var d = GetDistance(first.Tail, second.Head);

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
			var head = seq.Head;
			var tail = seq.Tail;

			double minDistance;
			int index;

			if (sequences.First() != seq)
			{
				minDistance = GetDistance(sequences[0].Head, head);
				index = 0;
			}
			else
			{
				minDistance = GetDistance(sequences[1].Head, head);
				index = 1;
			}

			foreach (var sequence in sequences.Where(s => s != seq))
			{
				var hh = GetDistance(sequence.Head, head);
				var ht = GetDistance(sequence.Head, tail);
				var tt = GetDistance(sequence.Tail, tail);
				var th = GetDistance(sequence.Tail, head);

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

		private static Segment FindByGradient(Segment obj, IEnumerable<Segment> objs)
		{
			var grad = obj.Centroid.Pixels[Layers.Gradient].Data[0];
			var result = objs.OrderBy(p => Math.Abs(grad - p.Centroid.Pixels[Layers.Gradient].Data[0]));
			return result.First();
		}

		private static double GetDistance(System.Windows.Point firstPoint, System.Windows.Point secondPoint)
		{
			var distance = Math.Pow(firstPoint.X - secondPoint.X, 2);
			distance += Math.Pow(firstPoint.Y - secondPoint.Y, 2);
			distance = Math.Sqrt(distance);

			return distance;
		}

		private bool IsSameColor(Segment firstSegment, Segment secondSegment)
		{
			var distance = Math.Abs(firstSegment.Centroid.Pixels[Layers.Original].Average - secondSegment.Centroid.Pixels[Layers.Original].Average);
			return distance < _tolerance;
		}

		private static double GetDistance(BrushStroke first, BrushStroke second)
		{
			var hh = GetDistance(first.Head, second.Head);
			var ht = GetDistance(first.Head, second.Tail);
			var tt = GetDistance(first.Tail, second.Tail);
			var th = GetDistance(first.Tail, second.Head);
			var minDistance = hh;

			if (ht < minDistance)
			{
				minDistance = ht;
			}

			if (tt < minDistance)
			{
				minDistance = tt;
			}

			if (th < minDistance)
			{
				minDistance = th;
			}

			return minDistance;
		}
	}
}