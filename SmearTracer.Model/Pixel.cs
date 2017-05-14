using System.Windows;

namespace SmearTracer.Model
{
    public class Pixel
    {
        public double[] ArgbArray;
        public Point Position;

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
