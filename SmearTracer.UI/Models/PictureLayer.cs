using System;
using System.Collections.Generic;
using SmearTracer.UI.Abstract;

namespace SmearTracer.UI.Models
{
    public class PictureLayer:Layer
    {
        private Lazy<List<Cluster>> _clustersLazy = new Lazy<List<Cluster>>(()=>UtilsInterface.CSplitter.Clustering());

        public override List<Cluster> Clusters
        {
            get => _clustersLazy.Value;
            set => _clustersLazy = new Lazy<List<Cluster>>(()=> value);
        }
    }
}
