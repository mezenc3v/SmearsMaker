using System.Collections.Generic;

namespace SmearTracer.Core.Models
{
    public class Canvas
    {
        public List<Layer> Layers;

        public Canvas()
        {
            Layers = new List<Layer>();
        }
    }
}
