using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SmearsMaker.Common.Image;

namespace GradientTracer.Analyzer
{
	public class GTImageModel : ImageModel
	{
		#region properties
		public ImageSetting FilterRank { get; }
		public ImageSetting HeightPlt { get; }
		public ImageSetting WidthPlt { get; }
		public ImageSetting SizeSuperPixel { get; }
		public ImageSetting HeightSmear { get; }
		public ImageSetting Tolerance { get; }
		#endregion

		public GTImageModel(BitmapSource image) : base(image)
		{
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

			Settings.AddRange(
				new List<ImageSetting>{
				FilterRank,
				HeightPlt,
				WidthPlt,
				SizeSuperPixel,
				HeightSmear,
				Tolerance});
		}
	}
}