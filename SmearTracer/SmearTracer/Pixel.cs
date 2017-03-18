using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmearTracer
{
    public class Pixel
    {
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Pixel()
        {

        }

        public Pixel(byte alpha, byte red, byte green, byte blue, int x, int y)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
            X = x;
            Y = y;
        }

        public Pixel(Color color, int x, int y)
        {
            Alpha = color.A;
            Red = color.R;
            Green = color.G;
            Blue = color.B;
            X = x;
            Y = y;
        }

        public Pixel(Pixel pixel)
        {
            Alpha = pixel.Alpha;
            Red = pixel.Red;
            Green = pixel.Green;
            Blue = pixel.Blue;
            X = pixel.X;
            Y = pixel.Y;
        }
    }
}
