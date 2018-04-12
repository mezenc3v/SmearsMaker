using SmearsMaker.ImageProcessing;

namespace SmearsMaker.Tracers
{
	public interface IServicesFactory
	{
		IStrokesBuilder CreateBsm();
		ISplitter CreateSplitter();
		IClusterizer CreateClusterizer();
		IFilter CreateFilter();
		IDetector CreateDetector();
	}
}