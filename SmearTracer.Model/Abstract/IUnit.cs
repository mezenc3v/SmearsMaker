using System.Windows;

namespace SmearTracer.Model.Abstract
{
    public interface IUnit
    {
        double[] ArgbArray { get; set; }
        Point Position { get; set; }
    }
}
