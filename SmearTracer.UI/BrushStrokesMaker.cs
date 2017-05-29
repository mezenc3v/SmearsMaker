using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SmearTracer.UI.Abstract;
using SmearTracer.UI.Models;

namespace SmearTracer.UI
{
    public class BrushStrokesMaker
    {
        private int _maxLemgth;
        private readonly Segment _segment;
        private Point _startPoint;
        private Point _finishPoint;

        public BrushStrokesMaker(Segment segment, int maxLemgth)
        {
            _segment = segment;
            _maxLemgth = maxLemgth;
        }

        public List<BrushStroke> Make()
        {
            FindPoints();

            var brushStrokes = new List<BrushStroke>();
            var points = _segment.GraphicUnits.Select(p => p.Center.Position).ToList();
            double length = 0;
            var list = new List<Point>();

            while (points.Count > 0)
            {
                
                var main = points.OrderBy(p => Distance(_finishPoint, p)).Last();
                list.Add(main);
                points.Remove(main);

                if (points.Count > 0)
                {
                    do
                    {
                        var next = points.OrderBy(p => Distance(list.Last(), p)).First();
                        length += Distance(list.Last(), next);
                        if (length <= _maxLemgth)
                        {
                            list.Add(next);
                            points.Remove(next);
                        }
                    } while (length <= _maxLemgth && points.Count > 0);

                    brushStrokes.Add(new BrushStroke { Points = list });
                    length = 0;
                    list = new List<Point>();
                }
                else
                {
                    brushStrokes.Add(new BrushStroke { Points = list });
                }

            }

            return brushStrokes;
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

            var start = _segment.MinX;
            var finish = _segment.MaxY;
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
        }

        private static double Distance(Point first, Point second)
        {
            var sum = Math.Pow(first.X - second.X, 2);
            sum += Math.Pow(first.Y - second.Y, 2);
            return Math.Sqrt(sum);
        }
    }
}
