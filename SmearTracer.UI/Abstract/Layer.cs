using System.Collections.Generic;

namespace SmearTracer.UI.Abstract
{
    public abstract class Layer
    {
        public abstract List<Cluster> Clusters { get; set; }
    }
}
