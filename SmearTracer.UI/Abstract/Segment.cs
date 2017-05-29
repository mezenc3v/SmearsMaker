using System.Collections.Generic;
using SmearTracer.UI.Models;

namespace SmearTracer.UI.Abstract
{
    public abstract class Segment:GraphicUnit
    {
        public abstract List<GraphicUnit> GraphicUnits { get; set; }

        public List<BrushStroke> BrushStrokes;
    }
}
