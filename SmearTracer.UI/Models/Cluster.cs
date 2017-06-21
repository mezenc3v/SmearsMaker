using System.Collections.Generic;
using SmearTracer.Core.Abstract;

namespace SmearTracer.Core.Models
{
    public class Cluster
    {
        public List<Segment> Segments;
        public double[] Centroid;
        public double[] LastCentroid;
        public List<IUnit> Data;

        public Cluster(int size)
        {
            Centroid = new double[size];
            LastCentroid = new double[size];
            Data = new List<IUnit>();
        }
    }
}
