using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SmearTracer.AL.Models
{
    public class PictureCanvas
    {
        public int Width;
        public int Height;
        public BitmapImage Source;
        public List<PictureLayer> Layers;
    }
}
