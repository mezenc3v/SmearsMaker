using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;

namespace SmearsMaker.SmearTracer
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
		public ImageSetting ClustersCount { get; set; }
		public ImageSetting MaxSmearDistance { get; set; }
		public ImageSetting MinSizeSuperpixel { get; set; }
		public ImageSetting MaxSizeSuperpixel { get; set; }
		public ImageSetting ClustersPrecision { get; set; }
		public ImageSetting ClusterMaxIteration { get; set; }
		public ImageSetting HeightPlt { get; set; }
		public ImageSetting WidthPlt { get; set; }
		#endregion

		public ImageModel(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");

			ClustersCount = new ImageSetting
			{
				Value = (int) (Math.Sqrt(Width + Height) + Math.Sqrt(Width + Height) / 2) + 3,
				Name = "Кол-во кластеров"
			};
			ClusterMaxIteration = new ImageSetting
			{
				Value = 100,
				Name = "Макс. число итераций класт."
			};
			MinSizeSuperpixel = new ImageSetting
			{
				Value = (int)(Width * Height / 1000),
				Name = "Мин. размер суперпикселя"
			};
			MaxSizeSuperpixel = new ImageSetting
			{
				Value = (int)(Width * Height / 500),
				Name = "Мaкс. размер суперпикселя"
			};
			MaxSmearDistance = new ImageSetting
			{
				Value = Width * Height,
				Name = "Максимальная длина мазка"
			};
			FilterRank = new ImageSetting
			{
				Value = (int)Math.Sqrt((double)(Width + Height) / 80 / 2),
				Name = "Ранг фильтра"
			};
			ClustersPrecision = new ImageSetting
			{
				Value = 0.001f,
				Name = "Точность поиска кластеров"
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
				ClustersCount,
				ClusterMaxIteration,
				MinSizeSuperpixel,
				MaxSizeSuperpixel,
				MaxSmearDistance,
				FilterRank,
				ClustersPrecision,
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