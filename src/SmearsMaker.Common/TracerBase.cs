using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NLog;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Common
{
	public abstract class TracerBase : ITracer
	{
		public IProgress Progress { get; }

		public abstract List<ImageSetting> Settings { get; }
		public abstract List<ImageView> Views { get; }
		protected ImageModel Model { get; }

		protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		protected TracerBase(BitmapSource image)
		{
			Model = new ImageModel(image);
			Progress = new ProgressImpl();
		}

		public abstract Task Execute();

		public abstract string CreatePlt();

		public virtual void Dispose()
		{
			Model?.Dispose();
		}
	}
}