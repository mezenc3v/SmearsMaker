using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmearTracer
{
    public class SuperPixel
    {
        public Point Centroid { get; set; }

        public List<Pixel> Data { get; set; }

        public SuperPixel(double x, double y)
        {
            Centroid = new Point(x, y);
            Data = new List<Pixel>();
        }

        public SuperPixel(Point point)
        {
            Centroid = point;
            Data = new List<Pixel>();
        }
    }
}
