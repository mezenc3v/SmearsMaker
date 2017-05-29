using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmearTracer.UI.Abstract;

namespace SmearTracer.UI.Models
{
    public class PictureCanvas:Canvas
    {
        public PictureCanvas()
        {
            Layers = new List<Layer>();
        }
    }
}
