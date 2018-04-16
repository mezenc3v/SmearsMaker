using System.Windows.Media.Imaging;
using SmearsMaker.Common;

namespace SmearsMaker.Tracers.GradientTracers
{
	public sealed class SquadGTracer : GTracer
	{
		public SquadGTracer(BitmapSource image, IProgress progress) : base(image)
		{
			Factory = new SquadTracerFactory(GtSettings, Model, progress);
		}
	}
}