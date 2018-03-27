using SmearsMaker.ImageProcessing;

namespace SmearsMaker.Tracers
{
	public interface IServicesFactory
	{
		IBsm CreateBsm();
		ISplitter CreateSplitter();
		IClusterizer CreateClusterizer();
		IFilter CreateFilter();
		IDetector CreateDetector();
	}
}