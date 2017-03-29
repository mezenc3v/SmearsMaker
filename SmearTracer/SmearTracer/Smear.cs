using System.Collections.Generic;

namespace SmearTracer
{
    public class Smear
    {
        public List<Circle> Circles { get; set; }

        public Smear()
        {
            Circles = new List<Circle>();
        }

        public bool Contains(Pixel point)
        {
            foreach (var circle in Circles)
            {
                if (circle.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }

        public int Contains(List<Pixel> data)
        {
            int counter = 0;
            foreach (var pixel in data)
            {
                if (Contains(pixel))
                {
                    counter++;
                }
            }
            return counter;
        }
    }
}
