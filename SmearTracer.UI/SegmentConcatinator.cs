using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmearTracer.UI.Abstract;
using SmearTracer.UI.Models;

namespace SmearTracer.UI
{
    public class SegmentConcatinator:Concatinator
    {
        private readonly List<Cluster> _clusters;
        private readonly int _minSize;

        public SegmentConcatinator(List<Cluster> clusters, int minSize)
        {
            _clusters = clusters;
            _minSize = minSize;
        }

        public override List<Segment> Concat()
        {
            throw new NotImplementedException();
        }
    }
}
