using System.Collections.Generic;

namespace SmearTracer.Core.Abstract
{
    public interface ISequenceOfPartMaker
    {
        List<SequenceOfParts> Execute();
    }
}
