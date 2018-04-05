using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.ImageProcessing
{
	public interface IFilter
	{
		PointCollection Filtering(PointCollection points);
	}
}