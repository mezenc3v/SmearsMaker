using System.Collections.Generic;
using SmearTracer.Model.Abstract;

namespace SmearTracer.Model
{
    public class Cluster:ICluster
    {
        public double[] Centroid { get; set; }
        public double[] LastCentroid { get; set; }
        public List<IUnit> Data { get; set; }

        public Cluster(int size)
        {
            Centroid = new double[size];
            LastCentroid = new double[size];
            Data = new List<IUnit>();
        }
    }
}
