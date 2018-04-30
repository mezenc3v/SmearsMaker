using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Common
{
	public interface ITracer : IDisposable
	{
		IProgress Progress { get; }
		List<ImageSetting> Settings { get;}
		List<ImageView> Views { get; }
		Task Execute();
		string GetPlt();
	}
}