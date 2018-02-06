using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace SmearsMaker.Common.Image
{
	public abstract class ImageModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public BitmapSource Image { get; }
		public int Width => Image.PixelWidth;
		public int Height => Image.PixelHeight;
		public List<ImageSetting> Settings { get; }
		protected ImageModel(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");
			Settings = new List<ImageSetting>();
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}