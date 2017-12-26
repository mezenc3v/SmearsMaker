using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;

namespace GradientTracer.Analyzer
{
	public class ImageModel : INotifyPropertyChanged
	{
		#region properties
		public event PropertyChangedEventHandler PropertyChanged;
		public BitmapSource Image { get; }

		public List<ImageSetting> Settings { get; }

		public int Width => Image.PixelWidth;
		public int Height => Image.PixelHeight;

		public ImageSetting FilterRank { get; set; }
		public ImageSetting HeightPlt { get; set; }
		public ImageSetting WidthPlt { get; set; }
		#endregion

		public ImageModel(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");

			FilterRank = new ImageSetting
			{
				Value = (int)Math.Sqrt((double)(Width + Height) / 80 / 2),
				Name = "Ранг фильтра"
			};
			
			HeightPlt = new ImageSetting
			{
				Value = 7600,
				Name = "Ширина plt"
			};
			WidthPlt = new ImageSetting
			{
				Value = 5200,
				Name = "Высота plt"
			};

			Settings = new List<ImageSetting>
			{
				FilterRank,
				HeightPlt,
				WidthPlt
			};
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}