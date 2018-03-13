using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.Helpers;
using SmearsMaker.Tracers.Model;

namespace SmearsMaker.Tracers.Logic
{
	public class GradientBsm : IBsm
	{
		private readonly Random _rnd;
		private double _maxDistance;
		private double _tolerance;

		public GradientBsm()
		{
			_rnd = new Random();
		}

		public List<BrushStroke> Execute(List<Segment> objs, double width, float toLerance)
		{
			_tolerance = toLerance;
			_maxDistance = width;
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

				var nearestPoints = points.FindAll(p => Utils.SqrtDistance(p.Centroid.Position, main.Centroid.Position) < _maxDistance);
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
					var strokes = NearestParts(sequences, sequences[i]);
					var distance = GetDistance(strokes.First(), sequences[i]);

					if (distance < _maxDistance)
					{
						var segmentToBeDeleted = FindByGradient(sequences[i], strokes);
						var combinedSequence = Combine(segmentToBeDeleted, sequences[i]);
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

			var a = Utils.SqrtDistance(first.Head, second.Head);
			var b = Utils.SqrtDistance(first.Tail, second.Tail);
			var c = Utils.SqrtDistance(first.Head, second.Tail);
			var d = Utils.SqrtDistance(first.Tail, second.Head);

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

		private static List<BrushStroke> NearestParts(IList<BrushStroke> sequences, BrushStroke seq)
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

		private static Segment FindByGradient(Segment obj, IEnumerable<Segment> objs)
		{
			var grad = obj.Centroid.Pixels[Layers.Gradient].Data[0];
			var result = objs.OrderBy(p => Math.Abs(grad - p.Centroid.Pixels[Layers.Gradient].Data[0]));
			return result.First();
		}

		private static BrushStroke FindByGradient(BrushStroke obj, IEnumerable<BrushStroke> objs)
		{
			var gradHead = obj.Objects.First().Centroid.Pixels[Layers.Gradient].Data[0];
			var gradTail = obj.Objects.Last().Centroid.Pixels[Layers.Gradient].Data[0];
			var grads = new List<float>();

			return objs.OrderBy(p =>
				{
					grads.Add(Math.Abs(gradHead - obj.Objects.First().Centroid.Pixels[Layers.Gradient].Data[0]));
					grads.Add(Math.Abs(gradTail - obj.Objects.Last().Centroid.Pixels[Layers.Gradient].Data[0]));
					grads.Add(Math.Abs(gradHead - obj.Objects.Last().Centroid.Pixels[Layers.Gradient].Data[0]));
					grads.Add(Math.Abs(gradTail - obj.Objects.First().Centroid.Pixels[Layers.Gradient].Data[0]));
					return grads.Min();
				}).First();
		}

		private bool IsSameColor(Segment firstSegment, Segment secondSegment)
		{
			var distance = Math.Abs(firstSegment.Centroid.Pixels[Layers.Original].Average - secondSegment.Centroid.Pixels[Layers.Original].Average);
			return distance < _tolerance;
		}

		private static double GetDistance(BrushStroke first, BrushStroke second)
		{
			return new List<double>
			{
				Utils.SqrtDistance(first.Head, second.Head),
				Utils.SqrtDistance(first.Head, second.Tail),
				Utils.SqrtDistance(first.Tail, second.Tail),
				Utils.SqrtDistance(first.Tail, second.Head)
			}.Min();
		}
	}
}