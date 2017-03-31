using System.Windows;

namespace SmearTracer
{
    public class Pixel
    {
        public double[] ArgbArray { get; set; }
        public Point PixelPosition;

        public Pixel()
        {
            PixelPosition = new Point();
        }

        public Pixel(Point point)
        {
            PixelPosition = point;
        }

        public Pixel(double[] argbArray, int x, int y)
        {
            ArgbArray = argbArray;
            PixelPosition = new Point(x, y);
        }

        public Pixel(double[] argbArray, Point point)
        {
            ArgbArray = argbArray;
            PixelPosition = point;
        }
    }
}
