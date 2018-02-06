using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NLog;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Helpers;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Common
{
	public abstract class TracerBase : ITracer
	{
		public Progress Progress { get; }
		public abstract List<ImageSetting> Settings { get; }
		public abstract List<ImageViewModel> Views { get; }

		protected readonly List<Point> Points;
		protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		protected TracerBase(BitmapSource image)
		{
			Points = Points = ImageHelper.ConvertToPixels(image);
			Progress = new Progress();
		}

		public abstract Task Execute();

		public abstract string GetPlt();
	}
}