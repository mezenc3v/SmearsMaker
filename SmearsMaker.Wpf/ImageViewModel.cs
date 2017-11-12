using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmearsMaker.Wpf
{
	public class ImageViewModel
	{
		public BitmapSource Source;
		public string Name;

		public ImageViewModel(BitmapSource source, string name)
		{
			Source = source;
			Name = name;
		}
	}
}