using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.StrokesFormation;
using SmearsMaker.Tracers.Extentions;
using SmearsMaker.Tracers.Helpers;
using SmearsMaker.Tracers.Model;

namespace SmearsMaker.Tracers.Logic
{
	public class GradientStrokesBuilder : IStrokesBuilder
	{
		private double _tolerance;
		private readonly Random _rnd;
		private readonly double _maxDistance;
		private readonly double _toleranceSecond;
		private readonly IProgress _progress;

		public GradientStrokesBuilder(IProgress progress, double width, float toleranceFirst, float toleranceSecond)
		{
			_toleranceSecond = toleranceSecond;
			_tolerance = toleranceFirst;
			_maxDistance = width;
			_progress = progress;
			_rnd = new Random();
		}

		public List<BrushStroke> Execute(List<Segment> objs)
		{
			
			var strokesFromGroups = new List<BrushStroke>();
			var groups = SplitSegmentsByColor(objs);
			_progress.NewProgress("Создание мазков", 0, groups.Count);
			Parallel.ForEach(groups, group =>
			{
				var pairs = Pairing(group);
				var brushStrokes = pairs.Count > 1 ? Combining(pairs) : pairs;
				
				lock (strokesFromGroups)
				{
					_progress.Update(1);
					strokesFromGroups.AddRange(brushStrokes);
				}
			});

			_tolerance = _toleranceSecond;

			return ConcatStrokes(strokesFromGroups);
		}

		private List<BrushStroke> ConcatStrokes(IEnumerable<BrushStroke> strokesFromGroups)
		{
			var groupsWithAloneStrokes = SplitSegmentsByColor(strokesFromGroups);

			var result = new List<BrushStroke>();
			_progress.NewProgress("Группировка одиночных суперпикселей", 0, groupsWithAloneStrokes.Count);
			foreach (var group in groupsWithAloneStrokes)
			{
				var sameColorStrokes = group.ToList();
				var aloneStrokes = sameColorStrokes.Where(stroke => stroke.Objects.Count == 1).ToList();
				//sameColorStrokes.RemoveAll(stroke => stroke.Objects.Count == 1);

				foreach (var aloneStroke in aloneStrokes)
				{
					if (sameColorStrokes.Any())
					{
						var strokes = aloneStroke.FindAllNearest(sameColorStrokes);
						if (strokes.Count > 0)
						{
							if (aloneStroke.GetDistance(strokes.First()) < _maxDistance)
							{
								var segmentToBeDeleted = aloneStroke.FindFirstNearestByLayer(strokes, Layers.Gradient);
								var combinedSequence = Combine(segmentToBeDeleted, aloneStroke);
								sameColorStrokes.Remove(segmentToBeDeleted);
								result.Add(combinedSequence);
							}
							else
							{
								result.Add(aloneStroke);
							}
						}
						else
						{
							result.Add(aloneStroke);
						}
					}
					else
					{
						result.Add(aloneStroke);
					}
				}
				result.AddRange(sameColorStrokes);
				_progress.Update(1);
			}

			return result;
		}

		private List<List<Segment>> SplitSegmentsByColor(IEnumerable<Segment> objs)
		{
			var groups = new List<List<Segment>>();
			var segments = new List<Segment>();
			segments.AddRange(objs);

			while (segments.Count > 0)
			{
				var segment = segments[_rnd.Next(0, segments.Count)];
				var searchableSegments = segments.FindAll(s => s.IsSameColor(segment, _tolerance, Layers.Gradient));
				var group = new List<Segment>();
				group.AddRange(searchableSegments);
				segments.RemoveAll(p => searchableSegments.Contains(p));
				segments.Remove(segment);
				groups.Add(group);
			}
			return groups;
		}

		private List<List<BrushStroke>> SplitSegmentsByColor(IEnumerable<BrushStroke> strokes)
		{
			var groups = new List<List<BrushStroke>>();
			var segments = new List<BrushStroke>();
			segments.AddRange(strokes);

			while (segments.Count > 0)
			{
				var segment = segments[_rnd.Next(0, segments.Count)];
				var searchableSegments = segments.FindAll(s => s.IsSameColor(segment, _tolerance));
				var group = new List<BrushStroke>();
				group.AddRange(searchableSegments);
				segments.RemoveAll(p => searchableSegments.Contains(p));
				segments.Remove(segment);
				groups.Add(group);
			}
			return groups;
		}

		private IList<BrushStroke> Pairing(IEnumerable<BaseShape> objs)
		{
			var brushStrokes = new List<BrushStroke>();
			var points = new List<BaseShape>();
			points.AddRange(objs);

			while (points.Count > 0)
			{
				var list = new List<BaseShape>();
				var main = points[_rnd.Next(0, points.Count)];

				list.Add(main);
				points.Remove(main);

				var nearestPoints = points.FindAll(p => Utils.SqrtDistance(p.GetCenter(), main.GetCenter()) < _maxDistance);
				if (nearestPoints.Any())
				{
					var next = main.FindNearest(nearestPoints, Layers.Gradient);
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
					var strokes = sequences[i].FindAllNearest(sequences);
					var distance = sequences[i].GetDistance(strokes.First());

					if (distance < _maxDistance)
					{
						var segmentToBeDeleted = sequences[i].FindFirstNearestByLayer(strokes, Layers.Gradient);
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
	}
}