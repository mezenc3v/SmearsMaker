using System;
using System.Collections.Generic;
using SmearsMaker.Common.Image;

namespace SmearTracer.Analyzer
{
	public class STImageSettings
	{
		#region properties
		public List<ImageSetting> Settings { get; }
		public ImageSetting FilterRank { get; }
		public ImageSetting ClustersCount { get; }
		public ImageSetting MaxSmearDistance { get; }
		public ImageSetting MinSizeSuperpixel { get; }
		public ImageSetting MaxSizeSuperpixel { get; }
		public ImageSetting ClustersPrecision { get; }
		public ImageSetting ClusterMaxIteration { get; }
		public ImageSetting HeightPlt { get; }
		public ImageSetting WidthPlt { get; }
		#endregion

		public STImageSettings(int width, int height)
		{
			ClustersCount = new ImageSetting
			{
				Value = (int)(Math.Sqrt(width + height) + Math.Sqrt(width + height) / 2) + 3,
				Name = "Кол-во кластеров"
			};
			ClusterMaxIteration = new ImageSetting
			{
				Value = 100,
				Name = "Макс. число итераций класт."
			};
			MinSizeSuperpixel = new ImageSetting
			{
				Value = (int)(width * height / 1000),
				Name = "Мин. размер суперпикселя"
			};
			MaxSizeSuperpixel = new ImageSetting
			{
				Value = (int)(width * height / 500),
				Name = "Мaкс. размер суперпикселя"
			};
			MaxSmearDistance = new ImageSetting
			{
				Value = width * height,
				Name = "Максимальная длина мазка"
			};
			FilterRank = new ImageSetting
			{
				Value = (int)Math.Sqrt((double)(width + height) / 80 / 2),
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
	}
}