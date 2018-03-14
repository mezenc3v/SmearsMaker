using System;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Tracers.Logic;

namespace SmearsMaker.Tracers.GradientTracers
{
	public sealed class HexagonGTracer : GTracer
	{
		public HexagonGTracer(BitmapSource image, IProgress progress) : base(image, progress, new HexagonSplitter(), new GradientBsm())
		{
		}

		internal override int SplitterLength => (int)GtSettings.WidthSmear.Value;
		internal override double BsmLength => GtSettings.WidthSmear.Value * Math.Sqrt(3) / 2 + 3;
	}
}