using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmearsMaker.Common.Image;
using SmearsMaker.HPGL;

namespace SmearsMaker.Common
{
	public interface ITracer : IPltCreator, IDisposable
	{
		IProgress Progress { get; }
		List<ImageSetting> Settings { get;}
		List<ImageView> Views { get; }
		Task Execute();
	}
}