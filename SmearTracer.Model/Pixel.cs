using System.Windows;
using SmearTracer.Model.Abstract;

namespace SmearTracer.Model
{
    public class Pixel:IUnit
    {
        public double[] ArgbArray { get; set; }
        public Point Position { get; set; }

        public Pixel()
        {
            Position = new Point();
        }

        public Pixel(Point position)
        {
            Position = position;
        }

        public Pixel(double[] argbArray, Point point)
        {
            ArgbArray = argbArray;
            Position = point;
        }
    }
}
