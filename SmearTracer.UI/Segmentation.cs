using System;
using System.Collections.Generic;
using System.Linq;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class Segmentation:ISegmentsSplitter
    {
        private readonly List<Segment> _segments;
        private readonly List<IUnit> _data;

        public Segmentation(List<IUnit> data)
        {
            _segments = new List<Segment>();
            _data = data;
        }

        private void Compute()
        {
            var data = _data.OrderBy(p => p.Position.X).ToList();

            while (data.Count > 0)
            {
                int countPrevious, countNext;
                var segment = new UnitSegment();
                segment.AddData(data[0]);
                data.RemoveAt(0);

                do
                {
                    var segmentData = new List<IUnit>();
                    countPrevious = data.Count;
                    foreach (var pixel in data.OrderBy(p=>Distance(p, segment.Units.Last())))
                    {
                        if (segment.Contains(pixel))
                        {
                            segment.AddData(pixel);
                        }
                        else
                        {
                            segmentData.Add(pixel);
                        }
                    }
                    data = segmentData;
                    countNext = segmentData.Count;
                } while (countPrevious != countNext);

                segment.Update();
                _segments.Add(segment);
            }
        }
        private double Distance(IUnit left, IUnit right)
        {
            var sum = Math.Pow(right.Position.X - left.Position.X, 2);
            sum += Math.Pow(right.Position.Y - left.Position.Y, 2);
            return Math.Sqrt(sum);
        }

        public List<Segment> Segmenting()
        {
            Compute();
            return _segments;
        }
    }
}
