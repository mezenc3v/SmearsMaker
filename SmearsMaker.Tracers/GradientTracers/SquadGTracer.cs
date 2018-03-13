using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Tracers.Logic;

namespace SmearsMaker.Tracers.GradientTracers
{
	public class SquadGTracer : GTracer
	{
		public SquadGTracer(BitmapSource image, IProgress progress) : base(image, progress)
		{
			ConfigureServices(new SuperpixelSplitter(), new GradientBsm());
		}

		protected override void PreExecute()
		{
			SplitterLength = (int)_settings.WidthSmear.Value;
			BsmLength = _settings.WidthSmear.Value * 2 - 2;
		}
	}
}