
using System.Windows;

namespace SmearTracer
{
    public class Pixel
    {
        public double[] Data { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Pixel(double[] data, int x, int y)
        {
            Data = data;
            X = x;
            Y = y;
        }

        public Pixel(double[] data, Point point)
        {
            Data = data;
            X = (int)point.X;
            Y = (int)point.Y;
        }
    }
}
