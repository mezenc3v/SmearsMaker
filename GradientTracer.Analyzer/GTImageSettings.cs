using System;
using System.Collections.Generic;
using SmearsMaker.Common.Image;

namespace GradientTracer.Analyzer
{
	public class GTImageSettings
	{
		#region properties
		public List<ImageSetting> Settings { get; }
		public ImageSetting FilterRank { get; }
		public ImageSetting HeightPlt { get; }
		public ImageSetting WidthPlt { get; }
		public ImageSetting SizeSuperPixel { get; }
		public ImageSetting WidthSmearUI { get; }
		public ImageSetting WidthSmear { get; }
		public ImageSetting Tolerance { get; }
		#endregion

		public GTImageSettings(int width, int height)
		{
			FilterRank = new ImageSetting
			{
				Value = (int)Math.Sqrt((double)(width + height) / 80 / 2),
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
				Value = (int)(width * height / 500),
				Name = "Размер суперпикселя"
			};
			WidthSmear = new ImageSetting
			{
				Value = width * height / 5000 + 1,
				Name = "Ширина мазка в plt"
			};
			WidthSmearUI = new ImageSetting
			{
				Value = width * height / 5000 + 1,
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
				WidthSmearUI,
				WidthSmear,
				Tolerance
			};
		}
	}
}