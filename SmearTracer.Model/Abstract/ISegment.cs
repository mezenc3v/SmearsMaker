using System.Collections.Generic;

namespace SmearTracer.Model.Abstract
{
    public interface ISegment:IGraphicUnit
    {
        List<IGraphicUnit> SuperPixels { get; set; }
        List<IFigure> CirclesList { get; set; }
    }
}
