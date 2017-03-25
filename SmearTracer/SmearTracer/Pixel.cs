
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
    }
}
