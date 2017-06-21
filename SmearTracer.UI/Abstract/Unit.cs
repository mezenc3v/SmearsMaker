using System.Windows;

namespace SmearTracer.Core.Abstract
{
    public interface IUnit
    {
        double[] Data { get; set; }
        Point Position { get; set; }
    }
}
