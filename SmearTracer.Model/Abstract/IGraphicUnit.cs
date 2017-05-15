using System.Collections.Generic;
using System.Windows;

namespace SmearTracer.Model.Abstract
{
    public interface IGraphicUnit
    {
        IUnit Center { get; set; }
        double[] Color { get; set; }
        Point MinX { get; set; }
        Point MinY { get; set; }
        Point MaxX { get; set; }
        Point MaxY { get; set; }
        List<IUnit> Data { get; set; }

        void Update();

        void AddData(IUnit pixel);

        bool Contains(IUnit pixel);

        void AddDataRange(IEnumerable<IUnit> pixels);
    }
}
