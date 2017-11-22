using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmearsMaker.Common
{
	public interface ITracer
	{
		List<ImageSetting> Settings { get;}
		Task Execute();
		List<ImageViewModel> Views { get; }
		string GetPlt();
	}
}