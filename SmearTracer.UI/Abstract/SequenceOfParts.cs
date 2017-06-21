using System.Collections.Generic;
using System.Linq;

namespace SmearTracer.Core.Abstract
{
    public abstract class SequenceOfParts
    {
        public double[] Color
        {
            get
            {
                var color = new double[Points.First().Data.Length];
                foreach (var point in Points)
                {
                    for (int i = 0; i < point.Data.Length; i++)
                    {
                        color[i] += point.Data[i];
                    }
                }
                for (int i = 0; i < color.Length; i++)
                {
                    color[i] /= Points.Count;
                }
                return color;
            }
        }

        public List<IUnit> Points;
        public int Width;

        public abstract double GetLength();
    }
}
