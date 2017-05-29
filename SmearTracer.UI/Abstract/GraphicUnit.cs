using System.Collections.Generic;
using System.Windows;
using SmearTracer.UI.Models;

namespace SmearTracer.UI.Abstract
{
    public abstract class GraphicUnit
    {
        public Unit Center;
        public double[] Color;
        public Point MinX;
        public Point MinY;
        public Point MaxX;
        public Point MaxY;
        public List<Unit> Units;

        public abstract void Update();

        public abstract void AddData(Unit unut);

        public abstract bool Contains(Unit unit);

        public abstract void AddDataRange(IEnumerable<Unit> units);

        public abstract double[] Generate();
    }
}
