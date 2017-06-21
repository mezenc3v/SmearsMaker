using System.Collections.Generic;

namespace SmearTracer.Core.Abstract
{
    public interface IFilter
    {
        List<IUnit> Filtering(List<IUnit> units);
    }
}
