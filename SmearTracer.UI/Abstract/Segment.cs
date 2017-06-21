using System.Collections.Generic;

namespace SmearTracer.Core.Abstract
{
    public abstract class Segment:Part
    {
        public List<Part> GraphicUnits;

        public List<SequenceOfParts> BrushStrokes;
    }
}
