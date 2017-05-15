using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SmearTracer.AL.Abstract
{
    public interface ISmear
    {
        List<Point> Points { get; set; }
        Color Color { get; set; }
    }
}