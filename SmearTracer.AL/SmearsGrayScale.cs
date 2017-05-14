using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SmearTracer.AL
{
    public struct Smear
    {
        public Point[] ArrayGrayScales;
        public Color Color;
    }

    public class SmearsGrayScale : ISmears
    {
        public SmearsGrayScale()
        {

        }

        public void Compute()
        {

        }

        public IList Smears()
        {
            var list = new List<Smear>();
            Smear smear;
            smear.ArrayGrayScales = new[]
            {
                new Point(1,1),
                new Point(3,4),
                new Point(6,7),
                new Point(3,6),
                new Point(4,5)
            };
            smear.Color = Colors.Black;
            list.Add(smear);
            return list;
        }
    }
}
