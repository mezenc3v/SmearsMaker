using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmearsMaker.Common;

namespace GradientTracer.Analyzer
{
	public class Tracer : ITracer
	{
		public List<ImageSetting> Settings { get; }
		public Task Execute()
		{
			throw new NotImplementedException();
		}

		public List<ImageViewModel> Views { get; }
		public string GetPlt()
		{
			throw new NotImplementedException();
		}
	}
}
