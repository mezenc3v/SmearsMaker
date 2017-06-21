using System.Collections.Generic;

namespace SmearTracer.Core.Abstract
{
    public interface ISegmentsSplitter
    {
        List<Segment> Segmenting();
    }
}
