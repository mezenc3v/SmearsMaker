using System.Windows.Media.Imaging;
using SmearsMaker.Common;

namespace SmearsMaker.Tracers.GradientTracers
{
	public sealed class HexagonGTracer : GTracer
	{
		public HexagonGTracer(BitmapSource image, IProgress progress) : base(image)
		{
			Factory = new HexagonTracerFactory(GtSettings, Model, progress);
		}
	}
}