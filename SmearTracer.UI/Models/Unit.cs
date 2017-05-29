using System.Windows;

namespace SmearTracer.UI.Models
{
    public class Unit
    {
        public double[] ArgbArray;
        public Point Position;

        public Unit()
        {
            Position = new Point();
        }

        public Unit(Point position)
        {
            Position = position;
        }

        public Unit(double[] argbArray, Point point)
        {
            ArgbArray = argbArray;
            Position = point;
        }
    }
}
