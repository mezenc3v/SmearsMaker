using System.Collections.Generic;
using SmearTracer.UI.Models;

namespace SmearTracer.UI.Abstract
{
    public abstract class Cluster
    {
        public double[] Centroid;
        public double[] LastCentroid;
        public List<Unit> Data;
        public abstract List<Segment> Segments { get; set; }
    }
}
