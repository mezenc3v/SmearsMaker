using System.Collections.Generic;
using System.Windows;
using SmearTracer.Core.Models;

namespace SmearTracer.Core.Abstract
{
    public abstract class Part
    {
        public Pixel Center;
        public double[] Color;
        public Point MinX;
        public Point MinY;
        public Point MaxX;
        public Point MaxY;
        public List<IUnit> Units;

        public abstract void Update();

        public abstract void AddData(IUnit unut);

        public abstract bool Contains(IUnit pixel);

        public abstract void AddDataRange(IEnumerable<IUnit> units);

        public abstract double[] Generate();
    }
}
