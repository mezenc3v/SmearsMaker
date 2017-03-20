using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
