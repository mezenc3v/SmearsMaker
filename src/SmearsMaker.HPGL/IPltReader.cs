using System.Windows.Media.Imaging;

namespace SmearsMaker.HPGL
{
	public interface IPltReader
	{
		BitmapSource ReadPlt(string path);
	}
}