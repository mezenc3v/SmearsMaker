using System;
using System.Collections.Generic;
using System.Linq;
using GradientTracer.Model;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;

namespace GradientTracer.Logic
{
	public class BsmPair
	{
		private readonly double _maxDistance;
		private readonly double _tolerance;

		public BsmPair(double maxDistance, float toLerance)
		{
			_tolerance = toLerance;
			_maxDistance = maxDistance;
		}

		public List<BrushStroke> Execute(List<Segment> objs)
		{
			var pairs = Pairing(objs);
			//var brushStrokes = Combining(pairs);
			return pairs;
		}

		private List<BrushStroke> Combining(List<BrushStroke> sequences)
		{
			bool distanceCheck;

			do
			{
				distanceCheck = false;
				for (int i = 0; i < sequences.Count; i++)
				{
					var head = sequences[i].Objects.First();
					var tail = sequences[i].Objects.Last();

					var trueSequenses = sequences.FindAll(
						s => ToLerance(s.Objects.First(), head) || ToLerance(s.Objects.First(), tail)
						     || ToLerance(s.Objects.Last(), head) || ToLerance(s.Objects.Last(), tail));
					if (trueSequenses.Any())
					{
						var index = NearestPart(trueSequenses, sequences[i]);
						//var index1 = FindByGradient(sequences[i], trueSequenses);

						if (Distance(sequences[i], sequences[index]) < _maxDistance)
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
			} while (distanceCheck && sequences.Count > 1);

			return sequences;
		}

		private static BrushStroke Combine(BrushStroke first, BrushStroke second)
		{
			var newSequence = new BrushStrokeImpl();

			var a = Distance(first.Objects.First().Centroid.Position, second.Objects.First().Centroid.Position);
			var b = Distance(first.Objects.Last().Centroid.Position, second.Objects.Last().Centroid.Position);
			var c = Distance(first.Objects.First().Centroid.Position, second.Objects.Last().Centroid.Position);
			var d = Distance(first.Objects.Last().Centroid.Position, second.Objects.First().Centroid.Position);

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
			var headPosition = seq.Objects.First().Centroid.Position;
			var tailPosition = seq.Objects.Last().Centroid.Position;

			double minDistance;
			int index;

			if (sequences.First() != seq)
			{
				minDistance = Distance(sequences[0].Objects.First().Centroid.Position, headPosition);
				index = 0;
			}
			else
			{
				minDistance = Distance(sequences[1].Objects.First().Centroid.Position, headPosition);
				index = 1;
			}

			foreach (var sequence in sequences.Where(s => s != seq))
			{
				var hh = Distance(sequence.Objects.First().Centroid.Position, headPosition);
				var ht = Distance(sequence.Objects.First().Centroid.Position, tailPosition);
				var tt = Distance(sequence.Objects.Last().Centroid.Position, tailPosition);
				var th = Distance(sequence.Objects.Last().Centroid.Position, headPosition);

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
			var minDistance = Distance(sequences.First().Objects.First().Centroid.Position, point.Position);

			for (int i = 0; i < sequences.Count; i++)
			{
				var distanceHead = Distance(sequences[i].Objects.First().Centroid.Position, point.Position);
				var distanceTail = Distance(sequences[i].Objects.Last().Centroid.Position, point.Position);

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

		private static BrushStroke Combine(BrushStroke first, Segment second)
		{
			var newSequence = new BrushStrokeImpl();

			var distanceHead = Distance(first.Objects.First().Centroid.Position, second.Centroid.Position);
			var distanceTail = Distance(first.Objects.Last().Centroid.Position, second.Centroid.Position);

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

		private List<BrushStroke> Pairing(IReadOnlyCollection<Segment> objs)
		{
			var brushStrokes = new List<BrushStroke>();
			var points = new List<Segment>();
			points.AddRange(objs);

			if (objs.Count > 1)
			{
				while (points.Count > 0)
				{
					if (points.Count > 1)
					{
						var list = new List<Segment>();
						var main = points.Last();

						list.Add(main);
						points.Remove(main);

						var truePoints = points.FindAll(p => Distance(p.Centroid.Position, main.Centroid.Position) < _maxDistance);
						if (truePoints.Any())
						{
							var next = FindByGradient(main, truePoints);
							//var next = truePoints.OrderBy(p => Distance(main.Centroid.Position, p.Centroid.Position)).First();
							if (next != null)
							{
								list.Add(next);
								points.Remove(next);
								brushStrokes.Add(new BrushStrokeImpl(list));
							}
							else
							{
								brushStrokes.Add(new BrushStrokeImpl(list));
							}

						}
						else if (brushStrokes.Any())
						{
							var index = FindNearestSequence(brushStrokes, list.First().Centroid);
							var newSequence = Combine(brushStrokes[index], list.First());

							brushStrokes.RemoveAt(index);
							brushStrokes.Add(newSequence);
						}
						else
						{
							brushStrokes.Add(new BrushStrokeImpl(list));
						}
					}
					else
					{
						var index = FindNearestSequence(brushStrokes, points.First().Centroid);
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

		private bool ToLerance(Segment next, Segment main)
		{
			//return Math.Abs(Math.Abs(next.Centroid.Pixels[FeatureDetection.BaseConsts.Gradient].Data[0]) -
			                //Math.Abs(main.Centroid.Pixels[FeatureDetection.BaseConsts.Gradient].Data[0])) < 30
			//return Math.Abs(Distance(next.Centroid.Pixels[GtLayers.Original].Data, main.Centroid.Pixels[GtLayers.Original].Data)) <= _tolerance;
			return Math.Abs(next.Centroid.Pixels[Layers.Gradient].Data[0] - main.Centroid.Pixels[Layers.Gradient].Data[0]) <= _tolerance;
		}

		private static Segment FindByGradient(Segment obj, List<Segment> objs)
		{		
			if (objs.Count == 1)
			{
				return objs.Single();
			}

			IEnumerable<Segment> result;

			var grad = obj.Centroid.Pixels[Layers.Gradient].Data[0];

			if (grad <= -22.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.X < obj.Centroid.Position.X && o.Centroid.Position.Y < obj.Centroid.Position.Y);
			}
			else if (grad <= 22.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.X < obj.Centroid.Position.X);
			}
			else if (grad <= 67.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.X < obj.Centroid.Position.X && o.Centroid.Position.Y > obj.Centroid.Position.Y);
			}
			else if (grad <= 112.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.Y > obj.Centroid.Position.Y);
			}
			else if (grad <= 157.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.X > obj.Centroid.Position.X && o.Centroid.Position.Y > obj.Centroid.Position.Y);
			}
			else if (grad <= 202.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.X > obj.Centroid.Position.X);
			}
			else if (grad <= 247.5)
			{
				result = objs.FindAll(o => o.Centroid.Position.X > obj.Centroid.Position.X && o.Centroid.Position.Y < obj.Centroid.Position.Y);
			}
			else
			{
				result = objs.FindAll(o => o.Centroid.Position.Y < obj.Centroid.Position.Y);
			}

			if (!result.Any())
			{
				return objs.OrderBy(ob => Distance(obj.Centroid.Position, ob.Centroid.Position)).First();
			}

			result = result.OrderBy(ob => Distance(obj.Centroid.Position, ob.Centroid.Position));
			
			return result.First();
		}

		private static double Distance(System.Windows.Point first, System.Windows.Point second)
		{
			var sum = Math.Pow(first.X - second.X, 2);
			sum += Math.Pow(first.Y - second.Y, 2);
			return Math.Sqrt(sum);
		}

		private static double Distance(BrushStroke first, BrushStroke second)
		{
			var hh = Distance(first.Objects.First().Centroid.Position, second.Objects.First().Centroid.Position);
			var ht = Distance(first.Objects.First().Centroid.Position, second.Objects.Last().Centroid.Position);
			var tt = Distance(first.Objects.Last().Centroid.Position, second.Objects.Last().Centroid.Position);
			var th = Distance(first.Objects.Last().Centroid.Position, second.Objects.First().Centroid.Position);
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