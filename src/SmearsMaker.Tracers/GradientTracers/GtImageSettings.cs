using System;
using System.Collections.Generic;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Tracers.GradientTracers
{
	public class GtImageSettings
	{
		#region properties
		public List<ImageSetting> Settings { get; }
		public ImageSetting FilterRank { get; }
		public ImageSetting HeightPlt { get; }
		public ImageSetting WidthPlt { get; }
		public ImageSetting WidthSmearUI { get; }
		public ImageSetting WidthSmear { get; }
		public ImageSetting Tolerance { get; }
		public ImageSetting Tolerance2 { get; }
		#endregion

		public GtImageSettings(int width, int height)
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
			WidthSmear = new ImageSetting
			{
				Value = width * height / 5000 + 1,
				Name = "Ширина мазка в plt"
			};
			WidthSmearUI = new ImageSetting
			{
				Value = width * height / 10000 + 1,
				Name = "Ширина мазка"
			};
			Tolerance = new ImageSetting
			{
				Value = 10,
				Name = "Погрешность в rgb"
			};

			Tolerance2 = new ImageSetting
			{
				Value = 13,
				Name = "Погрешность в rgb для коротких мазков"
			};

			Settings = new List<ImageSetting>
			{
				FilterRank,
				HeightPlt,
				WidthPlt,
				WidthSmearUI,
				WidthSmear,
				Tolerance,
				Tolerance2
			};
		}
	}
}