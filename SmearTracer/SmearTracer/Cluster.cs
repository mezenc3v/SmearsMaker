using System.Collections.Generic;

namespace SmearTracer
{
    public class Cluster
    {
        public double[] Centroid { get; set; }
        public double[] LastCentroid { get; set; }
        public List<Pixel> Data { get; set; }

        public Cluster(int size)
        {
            Centroid = new double[size];
            LastCentroid = new double[size];
            Data = new List<Pixel>();
        }
    }
}
