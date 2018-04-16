using SmearsMaker.ImageProcessing;

namespace SmearsMaker.Tracers
{
	public interface IServicesFactory
	{
		IStrokesBuilder CreateStrokesBuilder();
		ISplitter CreateSplitter();
		IClusterizer CreateClusterizer();
		IFilter CreateFilter();
		IDetector CreateDetector();
	}
}