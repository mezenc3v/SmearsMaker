using System.Collections.Generic;
using SmearTracer.Core.Abstract;

namespace SmearTracer.Core.Models
{
    public class Layer
    {
        public List<Cluster> Clusters;
        public List<IUnit> Data;

        public Layer(List<IUnit> units)
        {
            Data = units;
        }
    }
}
