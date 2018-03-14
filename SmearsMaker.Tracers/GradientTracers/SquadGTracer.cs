using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Tracers.Logic;

namespace SmearsMaker.Tracers.GradientTracers
{
	public sealed class SquadGTracer : GTracer
	{
		public SquadGTracer(BitmapSource image, IProgress progress) : base(image, progress, new SuperpixelSplitter(), new GradientBsm())
		{
		}

		internal override int SplitterLength => (int)GtSettings.WidthSmear.Value;
		internal override double BsmLength => GtSettings.WidthSmear.Value * 2 - 2;
	}
}