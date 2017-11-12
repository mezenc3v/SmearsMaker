using System.Windows.Media;

namespace SmearsMaker.Wpf
{
	public class ImageView
	{
		public ImageSource Source;
		public string Name;

		public ImageView(ImageSource source, string name)
		{
			Source = source;
			Name = name;
		}
	}
}