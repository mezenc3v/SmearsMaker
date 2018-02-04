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
		public ImageSetting SizeSuperPixel { get; set; }
		public ImageSetting HeightSmear { get; set; }
		public ImageSetting Tolerance { get; set; }
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

			SizeSuperPixel = new ImageSetting
			{
				Value = (int)(Width * Height / 500),
				Name = "Размер суперпикселя"
			};
			HeightSmear = new ImageSetting
			{
				Value = Width * Height / 5000 + 1,
				Name = "Ширина мазка"
			};
			Tolerance = new ImageSetting
			{
				Value = 10,
				Name = "Погрешность в rgb"
			};

			Settings = new List<ImageSetting>
			{
				FilterRank,
				HeightPlt,
				WidthPlt,
				SizeSuperPixel,
				HeightSmear,
				Tolerance
			};
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}