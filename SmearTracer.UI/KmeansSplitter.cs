using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class KmeansSplitter : IPartsSplitter
    {
        private readonly List<Part> _parts;
        private double _maxSize;
        private int _minSize;
        private readonly Segment _segment;

        public KmeansSplitter(Segment segment, int maxSize)
        {
            _parts = new List<Part>();
            _maxSize = maxSize;
            _minSize = maxSize;
            _segment = segment;
        }

        private void UpdateMeans()
        {
            foreach (var part in _parts)
            {
                var x = part.Units.Sum(p => p.Position.X) / part.Units.Count;
                var y = part.Units.Sum(p => p.Position.Y) / part.Units.Count;

                part.Center.Position = new Point(x, y);
                part.Units = new List<IUnit>();
            }
            foreach (var unit in _segment.Units)
            {
                var index = NearestCentroid(unit);
                _parts[index].Units.Add(unit);
            }

            _parts.RemoveAll(c => c.Units.Count == 0);
        }

        private int NearestCentroid(IUnit unit)
        {
            var index = 0;
            var min = Distance(unit.Position, _parts.First().Center.Position);
            for (int i = 0; i < _parts.Count; i++)
            {
                var distance = Distance(unit.Position, _parts[i].Center.Position);

                if (min > distance)
                {
                    min = distance;
                    index = i;
                }
            }
            return index;
        }

        private static double Distance(Point leftVector, Point rightVector)
        {
            double distance = 0;

            var dx = leftVector.X - rightVector.X;
            if (dx < 0)
            {
                distance -= dx;
            }
            else
            {
                distance += dx;
            }

            var dy = leftVector.Y - rightVector.Y;
            if (dy < 0)
            {
                distance -= dy;
            }
            else
            {
                distance += dy;
            }

            //distance = Math.Sqrt(Math.Pow(leftVector.X - rightVector.X, 2) + Math.Pow(leftVector.Y - rightVector.Y, 2));

            return distance;
        }

        private void Split(Part part)
        {
            var data = part.Units;
            var firstPartData = new List<IUnit>();
            var secondPartData = new List<IUnit>();

            for (int i = 0; i < data.Count; i++)
            {
                if (i < data.Count / 2)
                {
                    firstPartData.Add(data[i]);
                }
                else
                {
                    secondPartData.Add(data[i]);
                }
            }

            var firstPart = new Superpixel(new Point())
            {
                Units = firstPartData
            };
            var secondPart = new Superpixel(new Point())
            {
                Units = secondPartData
            };

            firstPart.Update();
            secondPart.Update();

            _parts.Remove(part);
            _parts.Add(firstPart);
            _parts.Add(secondPart);
        }

        private bool CheckSize(Part part)
        {
            //var minLength = Math.Sqrt(_maxSize) - 1;
            //var width = (int)(part.MaxX.X - part.MinX.X);
            //var height = (int)(part.MaxY.Y - part.MinY.Y);

            if (part.Units.Count < _minSize / 4 /*|| width * height > part.Units.Count * 9 / 10*/)
            {
                return false;
            }
            return true;
        }

        private bool CheckForm(Part part)
        {
            var width = (int)(part.MaxX.X - part.MinX.X);
            var height = (int)(part.MaxY.Y - part.MinY.Y);

            if (width / (height + 1) > 4 || height / (width + 1) > 4)
            {
                return false;
            }
            return true;
        }

        public List<Part> Splitting()
        {
            bool checkSize;

            /*var initialPart = new Superpixel(new Point())
            {
                Units = _segment.Units
            };
            initialPart.Update();

            _parts.Add(initialPart);*/

            var firstPoint = new Point(_segment.MinX.X, _segment.MinY.Y);
            var secondPoint = new Point(_segment.MaxX.X, _segment.MaxY.Y);
            var widthCount = (int)(_segment.MaxX.X - _segment.MinX.X);
            var heightCount = (int)(_segment.MaxY.Y - _segment.MinY.Y);
            var samples = InitilalCentroids(widthCount, heightCount, firstPoint, secondPoint, Math.Sqrt(_minSize));

            _parts.AddRange(samples.Select(centroid => new Superpixel(centroid)));

            foreach (var unit in _segment.Units)
            {
                var index = NearestCentroid(unit);
                _parts[index].Units.Add(unit);
            }
            _parts.RemoveAll(c => c.Units.Count == 0);
            var counter = 0;
            do
            {
                checkSize = true;

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < _parts.Count; j++)
                    {
                        if (_parts[j].Units.Count > _maxSize)
                        {
                            checkSize = false;
                            Split(_parts[j]);
                            //break;
                        }
                        else if (!CheckSize(_parts[j]) && _parts.Count > 1)
                        {
                            checkSize = false;
                            //_segment.Units.RemoveAll(p=>part.Contains(p));
                            _parts.RemoveAt(j);
                            //break;
                        }
                        else if (!CheckForm(_parts[j]))
                        {
                            checkSize = false;
                            //_segment.Units.RemoveAll(p => _parts[j].Contains(p));
                            //_parts.RemoveAt(j);
                            Split(_parts[j]);
                        }
                        
            /*else if (_parts.Count == 1)
            {
                return new List<Part>();
            }*/
            }

                    UpdateMeans();
                }

                _maxSize *= 1.1;
                counter++;
            } while (checkSize == false && counter < 300);
            //UpdateMeans();

            _parts.RemoveAll(p => p.Units.Count == 0);

            return _parts;
        }

        private static IEnumerable<Point> InitilalCentroids(int widthCount, int heightCount, Point firstPoint, Point secondPoint, double diameter)
        {
            var samplesData = new List<Point>();

            if (widthCount > diameter && heightCount > diameter)
            {
                for (int i = (int)firstPoint.X; i < (int)secondPoint.X; i += (int)diameter)
                {
                    for (int j = (int)firstPoint.Y; j < (int)secondPoint.Y; j += (int)diameter)
                    {
                        samplesData.Add(new Point(i + diameter / 2, j + diameter / 2));
                    }
                }
            }
            else
            {
                samplesData.Add(new Point((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2));
            }

            return samplesData;
        }

    }
}
