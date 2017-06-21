using System.Windows;
using SmearTracer.Core.Abstract;

namespace SmearTracer.Core.Models
{
    public class Pixel:IUnit
    {
        public double[] Data { get; set; }
        public Point Position { get; set; }

        public Pixel()
        {
            Position = new Point();
        }

        public Pixel(Point position)
        {
            Position = position;
        }

        public Pixel(double[] data, Point point)
        {
            Data = data;
            Position = point;
        }
    }
}
