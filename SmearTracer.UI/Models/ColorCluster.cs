using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmearTracer.UI.Abstract;

namespace SmearTracer.UI.Models
{
    public class ColorCluster:Cluster
    {
        private Lazy<List<Segment>> _segmentsLazy = new Lazy<List<Segment>>(() => UtilsInterface.SSplitter.Segmenting());

        public ColorCluster(int size)
        {
            Centroid = new double[size];
            LastCentroid = new double[size];
            Data = new List<Unit>();
        }

        public override List<Segment> Segments
        {
            get =>_segmentsLazy.Value;
            set => _segmentsLazy = new Lazy<List<Segment>>(()=> value);
        }
    }
}
