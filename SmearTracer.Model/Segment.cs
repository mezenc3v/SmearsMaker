using System.Collections.Generic;

namespace SmearTracer.Model
{
    public class Segment : GraphicUnit
    {
        public List<SuperPixel> SuperPixels;
        public List<Circle> CirclesList;

        public Segment()
        {
            Center = new Pixel();
            Data = new List<Pixel>();
            SuperPixels = new List<SuperPixel>();
            CirclesList = new List<Circle>();
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
