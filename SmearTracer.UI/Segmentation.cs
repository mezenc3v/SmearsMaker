using System;
using System.Collections.Generic;
using System.Linq;
using SmearTracer.UI.Abstract;
using SmearTracer.UI.Models;

namespace SmearTracer.UI
{
    public class Segmentation:SegmentSplitter
    {
        private readonly List<UnitSegment> _segments;
        private readonly List<Unit> _data;
        private readonly int _minSize;

        public Segmentation(List<Unit> data, int minSize)
        {
            _segments = new List<UnitSegment>();
            _data = data;
            _minSize = minSize;
        }

        private void Compute()
        {
            var segmentData = _data.OrderBy(d => d.Position.X).ToList();
            while (segmentData.Count > 0)
            {
                var data = segmentData;
                int countPrevious, countNext;
                var segment = new UnitSegment();
                segment.AddData(data[0]);
                data.RemoveAt(0);
                do
                {
                    segmentData = new List<Unit>();
                    countPrevious = data.Count;
                    foreach (var pixel in data)
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

        public override List<Segment> Segmenting()
        {
            Compute();
            return _segments.ToList<Segment>();
        }
    }
}
