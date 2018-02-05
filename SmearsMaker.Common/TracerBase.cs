using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Common
{
	public abstract class TracerBase : ITracer
	{
		public abstract List<ImageSetting> Settings { get; }
		public abstract List<ImageViewModel> Views { get; }

		protected ImageModel _data;
		protected List<Smear> _smears;
		protected Progress _progress;
		protected static ILogger Log = LogManager.GetCurrentClassLogger();

		public abstract Task Execute();

		public abstract string GetPlt();
	}
}