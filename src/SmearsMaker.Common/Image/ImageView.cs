using System.Windows.Media.Imaging;

namespace SmearsMaker.Common.Image
{
	public class ImageView
	{
		public BitmapSource Source;
		public string Name;

		public ImageView(BitmapSource source, string name)
		{
			Source = source;
			Name = name;
		}
	}
}