using System.Collections.Generic;

namespace SmearTracer.Model
{
    public class Cluster
    {
        public double[] Centroid;
        public double[] LastCentroid;
        public List<Pixel> Data;

        public Cluster(int size)
        {
            Centroid = new double[size];
            LastCentroid = new double[size];
            Data = new List<Pixel>();
        }
    }
}
