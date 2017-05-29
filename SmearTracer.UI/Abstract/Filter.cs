using System.Collections.Generic;
using SmearTracer.UI.Models;

namespace SmearTracer.UI.Abstract
{
    public abstract class Filter
    {
        public abstract List<Unit> Filtering(List<Unit> units);
    }
}
