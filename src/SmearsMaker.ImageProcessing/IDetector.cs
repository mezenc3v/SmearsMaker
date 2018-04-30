using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing
{
	public interface IDetector
	{
		PointCollection Compute(PointCollection points);
	}
}