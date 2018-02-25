using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NLog;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Common
{
	public abstract class TracerBase : ITracer
	{
		public Progress Progress { get; }
		public ImageModel Model { get; }

		public abstract List<ImageSetting> Settings { get; }
		public abstract List<ImageView> Views { get; }

		protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		protected TracerBase(BitmapSource image)
		{
			Model = new ImageModel(image);
			Progress = new Progress();
		}

		public abstract Task Execute();

		public abstract string GetPlt();
	}
}