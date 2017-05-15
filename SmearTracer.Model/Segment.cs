using System.Collections.Generic;
using SmearTracer.Model.Abstract;

namespace SmearTracer.Model
{
    public class Segment : GraphicUnit, ISegment
    {
        public List<IGraphicUnit> SuperPixels { get; set; }
        public List<IFigure> CirclesList { get; set; }

        public Segment()
        {
            Center = new Pixel();
            Data = new List<IUnit>();
            SuperPixels = new List<IGraphicUnit>();
            CirclesList = new List<IFigure>();
        }

        public Segment(Segment segment)
        {
            Center = segment.Center;
            Data = segment.Data;
            Color = Generate();
            MinX = segment.MinX;
            MaxX = segment.MaxX;
            MinY = segment.MinY;
            MaxY = segment.MaxY;
            SuperPixels = segment.SuperPixels;
            CirclesList = segment.CirclesList;
        }
    }
}
