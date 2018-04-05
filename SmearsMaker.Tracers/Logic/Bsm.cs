using System;
using System.Collections.Generic;
using System.Linq;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.ImageProcessing.Segmenting;
using SmearsMaker.ImageProcessing.SmearsFormation;
using SmearsMaker.Tracers.Helpers;
using SmearsMaker.Tracers.Model;
using Point = System.Windows.Point;

namespace SmearsMaker.Tracers.Logic
{
	public class Bsm : IBsm
	{
		private readonly double _maxLemgth;
		private Point _finishPoint;

		public Bsm(double width)
		{
			_maxLemgth = width;
		}

		public List<BrushStroke> Execute(List<Segment> objs)
		{
			FindPoints(objs);
			if (objs.Count > 0)
			{
				var size = Math.Sqrt(objs.First().Points.Count);
				var brushStrokes = new List<BrushStroke>();
				var points = new List<Segment>();
				points.AddRange(objs);

				double length = 0;
				var list = new List<BaseShape>();

				while (points.Count > 0)
				{
					var main = points.OrderBy(p => Utils.SqrtDistance(_finishPoint, p.GetCenter())).First();
					list.Add(main);
					points.Remove(main);

					if (points.Count > 0)
					{
						do
						{
							var next = points.OrderBy(p => Utils.SqrtDistance(list.Last().GetCenter(), p.GetCenter())).First();
							if (Utils.SqrtDistance(list.Last().GetCenter(), next.GetCenter()) / 2 < size)
							{
								length += Utils.SqrtDistance(list.Last().GetCenter(), next.GetCenter());
								if (length <= _maxLemgth)
								{
									_finishPoint = next.GetCenter();
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
							list = new List<BaseShape>();
						}
						else
						{
							length = 0;
							list = new List<BaseShape>
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

		private void FindPoints(IReadOnlyCollection<BaseShape> objs)
		{
			var finish = objs.First().Points.First().Position;
			double maxDistance = 0;

			foreach (var objOne in objs)
			{
				foreach (var objTwo in objs)
				{
					if (Utils.SqrtDistance(objOne.GetCenter(), objTwo.GetCenter()) > maxDistance)
					{
						maxDistance = Utils.SqrtDistance(objOne.GetCenter(), objTwo.GetCenter());
						finish = objTwo.GetCenter();
					}
				}
			}
			_finishPoint = finish;
		}
	}
}
