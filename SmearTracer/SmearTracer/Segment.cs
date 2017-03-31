using System.Collections.Generic;

namespace SmearTracer
{
    public class Segment
    {
        public Information Information { get; }
        public List<SuperPixel> SuperPixels { get; set; }
        public List<Circle> CirclesList { get; set; }

        public Segment()
        {
            Information = new Information();
            SuperPixels = new List<SuperPixel>();
            CirclesList = new List<Circle>();
        }

        public Segment(Segment inputSegment)
        {
            Information = new Information(inputSegment.Information);
            SuperPixels = inputSegment.SuperPixels;
            CirclesList = inputSegment.CirclesList;
        }

    }
}
