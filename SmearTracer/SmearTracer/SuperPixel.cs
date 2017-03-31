using System.Windows;

namespace SmearTracer
{
    public class SuperPixel
    {
        public Information Information { get; }

        public SuperPixel(double x, double y)
        {
            var inf = new Information(x, y);
            Information = inf;
        }

        public SuperPixel(Point centerPoint)
        {
            var inf = new Information(centerPoint);
            Information = inf;
        }

        public SuperPixel(SuperPixel superPixel)
        {
            Information = new Information(superPixel.Information);
        }
    }
}
