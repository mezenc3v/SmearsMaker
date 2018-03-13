using System;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Tracers.Logic;

namespace SmearsMaker.Tracers.GradientTracers
{
	public class HexagonGTracer : GTracer
	{
		public HexagonGTracer(BitmapSource image, IProgress progress) : base(image, progress)
		{
			ConfigureServices(new HexagonSplitter(), new GradientBsm());
		}

		protected override void PreExecute()
		{
			SplitterLength = (int)_settings.WidthSmear.Value;
			BsmLength = _settings.WidthSmear.Value * Math.Sqrt(3) / 2 + 4;
		}
	}
}