using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using SmearTracer.AL.Abstract;

namespace SmearTracer.AL
{
    public class Smear:ISmear
    {
        public List<Point> Points { get; set; }
        public Color Color { get; set; }
    }
}