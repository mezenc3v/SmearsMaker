using System.Windows.Media.Imaging;
using SmearsMaker.Common;

namespace SmearsMaker.Tracers.GradientTracers
{
	public sealed class SquadGTracer : GTracer
	{
		public SquadGTracer(BitmapSource image) : base(image)
		{
			Factory = new SquadTracerFactory(GtSettings, Model, Progress);
		}
	}
}