using System.Collections;

namespace SmearTracer.AL
{
    public interface ISmears
    {
        void Compute();
        IList Smears();
    }
}
