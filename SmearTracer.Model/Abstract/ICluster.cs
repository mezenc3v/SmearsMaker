using System.Collections.Generic;

namespace SmearTracer.Model.Abstract
{
    public interface ICluster
    {
        double[] Centroid { get; set; }
        double[] LastCentroid { get; set; }
        List<IUnit> Data { get; set; }
    }
}
