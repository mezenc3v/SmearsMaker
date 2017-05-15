using System.Collections.Generic;
using System.Windows;

namespace SmearTracer.Model.Abstract
{
    public interface IFigure
    {
        Point Center { get; set; }
        double Radius { get; set; }
        double[] Color { get; set; }
        bool Contains(IUnit pixel);
        bool Contains(Point pixel);
        int Contains(List<IUnit> data);
    }
}
