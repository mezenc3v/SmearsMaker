using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Tracers.Model;
using Point = System.Windows.Point;

namespace SmearsMaker.Tracers.SmearTracer.Logic
{
	public class Bsm
	{
		private readonly int _maxLemgth;
		private Point _finishPoint;

		public Bsm(int maxLemgth)
		{
			_maxLemgth = maxLemgth;
		}

		public List<BrushStroke> Execute(List<Segment> objs)
		{
			FindPoints(objs);
			if (objs.Count > 0)
			{
				var size = Math.Sqrt(objs.First().Data.Count);
				var brushStrokes = new List<BrushStroke>();
				var points = new List<Segment>();
				points.AddRange(objs);

				double length = 0;
				var list = new List<Segment>();

				while (points.Count > 0)
				{
					var main = points.OrderBy(p => Distance(_finishPoint, p.Centroid.Position)).First();
					list.Add(main);
					points.Remove(main);

					if (points.Count > 0)
					{
						do
						{
							var next = points.OrderBy(p => Distance(list.Last().Centroid.Position, p.Centroid.Position)).First();
							if (Distance(list.Last().Centroid.Position, next.Centroid.Position) / 2 < size)
							{
								length += Distance(list.Last().Centroid.Position, next.Centroid.Position);
								if (length <= _maxLemgth)
								{
									_finishPoint = next.Centroid.Position;
									list.Add(next);
									points.Remove(next);
								}
							}
							else
							{
								break;
							}
						} while (length <= _maxLemgth && points.Count > 0);

						brushStrokes.Add(new BrushStrokeImpl(list));

						if (length <= _maxLemgth)
						{
							length = 0;
							list = new List<Segment>();
						}
						else
						{
							length = 0;
							list = new List<Segment>
							{
								brushStrokes.Last().Objects.Last()
							};
						}
					}
					else
					{
						brushStrokes.Add(new BrushStrokeImpl(list));
					}

				}
				return brushStrokes;
			}

			return new List<BrushStroke>();
		}

		private void FindPoints(IEnumerable<Segment> objs)
		{
			var finish = objs.First().MinX;
			double maxDistance = 0;

			foreach (var objOne in objs)
			{
				foreach (var objTwo in objs)
				{
					if (Distance(objOne.Centroid.Position, objTwo.Centroid.Position) > maxDistance)
					{
						maxDistance = Distance(objOne.Centroid.Position, objTwo.Centroid.Position);
						finish = objTwo.Centroid.Position;
					}
				}
			}
			_finishPoint = finish;
		}

		private static double Distance(Point first, Point second)
		{
			var sum = Math.Pow(first.X - second.X, 2);
			sum += Math.Pow(first.Y - second.Y, 2);
			return Math.Sqrt(sum);
		}
	}
}
