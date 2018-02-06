using System.Collections.Generic;
using System.Threading.Tasks;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Common
{
	public interface ITracer
	{
		Progress Progress { get; }
		List<ImageSetting> Settings { get;}
		Task Execute();
		List<ImageViewModel> Views { get; }
		string GetPlt();
	}
}