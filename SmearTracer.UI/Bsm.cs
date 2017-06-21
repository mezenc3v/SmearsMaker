using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class Bsm:ISequenceOfPartMaker
    {
        private readonly int _maxLemgth;
        private readonly Segment _segment;
        private Point _startPoint;
        private Point _finishPoint;

        public Bsm(Segment segment, int maxLemgth)
        {
            _segment = segment;
            _maxLemgth = maxLemgth;
        }

        public List<SequenceOfParts> Execute()
        {
            FindPoints();
            if (_segment.GraphicUnits.Count > 0)
            {
                var size = Math.Sqrt(_segment.GraphicUnits.First().Units.Count);
                var brushStrokes = new List<SequenceOfParts>();
                var points = _segment.GraphicUnits.Select(p => p.Center).ToList();
                double length = 0;
                var list = new List<IUnit>();

                while (points.Count > 0)
                {
                    var main = points.OrderBy(p => Distance(_finishPoint, p.Position)).First();
                    list.Add(main);
                    points.Remove(main);

                    if (points.Count > 0)
                    {
                        do
                        {
                            var next = points.OrderBy(p => Distance(list.Last().Position, p.Position)).First();
                            if (Distance(list.Last().Position, next.Position) / 2 < size)
                            {
                                length += Distance(list.Last().Position, next.Position);
                                if (length <= _maxLemgth)
                                {
                                    _finishPoint = next.Position;
                                    list.Add(next);
                                    points.Remove(next);
                                }
                            }
                            else
                            {
                                break;
                            }
                        } while (length <= _maxLemgth && points.Count > 0);

                        brushStrokes.Add(new BrushStroke { Points = list });

                        if (length <= _maxLemgth)
                        {
                            length = 0;
                            list = new List<IUnit>();
                        }
                        else
                        {
                            length = 0;
                            list = new List<IUnit>
                            {
                                brushStrokes.Last().Points.Last()
                            };
                        }
                    }
                    else
                    {
                        brushStrokes.Add(new BrushStroke { Points = list });
                    }

                }
                return brushStrokes;
            }

            return new List<SequenceOfParts>();
        }

        private void FindPoints()
        {
            var distances = new List<Point>
            {
                _segment.MaxX,
                _segment.MaxY,
                _segment.MinX,
                _segment.MinY
            };

            //var start = new Point(distances.Min(p => p.X), distances.Min(p => p.Y));
            //var finish = new Point(distances.Max(p => p.X), distances.Max(p => p.Y));
            var start = _segment.MinX;
            var finish = _segment.MinX;
            double maxDistance = 0;

            foreach (var pointOne in distances)
            {
                foreach (var pointTwo in distances)
                {
                    if (Distance(pointOne, pointTwo) > maxDistance)
                    {
                        maxDistance = Distance(pointOne, pointTwo);
                        start = pointOne;
                        finish = pointTwo;
                    }
                }
            }
            _startPoint = start;
            _finishPoint = finish;

            _finishPoint = _segment.Center.Position;
        }

        private static double Distance(Point first, Point second)
        {
            var sum = Math.Pow(first.X - second.X, 2);
            sum += Math.Pow(first.Y - second.Y, 2);
            return Math.Sqrt(sum);
        }
    }
}
