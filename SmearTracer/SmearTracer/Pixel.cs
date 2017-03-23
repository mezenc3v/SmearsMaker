
namespace SmearTracer
{
    public class Pixel
    {
        public double[] Data { get; set; }
        public double[] Coordinates { get; set; }

        public Pixel(double[] data, int x, int y)
        {
            Data = data;
            Coordinates = new double[] { x, y };
        }
    }
}
