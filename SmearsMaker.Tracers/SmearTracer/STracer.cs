using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing.Clustering;
using SmearsMaker.ImageProcessing.Filtering;
using SmearsMaker.Tracers.Helpers;
using SmearsMaker.Tracers.Logic;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace SmearsMaker.Tracers.SmearTracer
{
	public class STracer : TracerBase
	{
		public override List<ImageSetting> Settings => _settings.Settings;
		public override List<ImageView> Views => CreateViews();

		private readonly StImageSettings _settings;
		private List<Smear> _smears;

		public STracer(BitmapSource image, IProgress progress) : base(image, progress)
		{
			_settings = new StImageSettings(Model.Width, Model.Height);
		}

		public override Task Execute()
		{
			var filter = new MedianFilter((int)_settings.FilterRank.Value, Model.Width, Model.Height);
			var kmeans = new KmeansClassic((int)_settings.ClustersCount.Value, _settings.ClustersPrecision.Value, Model.Points, (int)_settings.ClusterMaxIteration.Value);
			var supPixSplitter = new SuperpixelSplitter();
			var bsm = new BsmPair();

			var segmentsCount = 0;
			var smearsCount = 0;

			return Task.Run(() =>
			{
				_smears = new List<Smear>();
				Log.Trace("Начало обработки изображения");
				Progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				filter.Filter(Model.Points);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				Progress.NewProgress("Кластеризация");
				var clusters = kmeans.Clustering();
				Log.Trace($"Кластеризация заняла {sw.Elapsed.Seconds} с.");
				var defectedPixels = new List<Point>();
				Progress.NewProgress("Обработка", 0, clusters.Sum(c => c.Data.Count));
				Parallel.ForEach(clusters, (cluster) =>
				{
					var swClusters = Stopwatch.StartNew();
					var segments = SegmentSplitter.Split(cluster.Data);

					segmentsCount += segments.Count;
					Log.Trace($"Сегментация кластера размером {cluster.Data.Count} пикселей заняла {swClusters.Elapsed.Seconds} с.");
					swClusters.Reset();
					Parallel.ForEach(segments, segment =>
					{
						var superPixels = supPixSplitter.Splitting(segment, (int)_settings.MinSizeSuperpixel.Value);

						Parallel.ForEach(superPixels, (supPix) =>
						{
							if (supPix.Data.Count < _settings.MinSizeSuperpixel.Value)
							{
								lock (defectedPixels)
								{
									defectedPixels.AddRange(supPix.Data);
								}
							}
						});

						if (superPixels.Count > 0)
						{
							var smears = bsm.Execute(superPixels.ToList(), _settings.MaxSmearDistance.Value, 0, 0);
							smearsCount += smears.Count;
							foreach (var smear in smears)
							{
								var newSmear = new Smear(smear)
								{
									Cluster = cluster,
									Segment = segment
								};

								lock (_smears)
								{
									_smears.Add(newSmear);
								}
								Progress.Update(newSmear.BrushStroke.Objects.Sum(o => o.Data.Count));
							}
						}
					});
					Log.Trace($"Разбиение на мазки кластера размером {cluster.Data.Count} пикселей заняло {swClusters.Elapsed.Seconds} с.");
				});

				if (defectedPixels.Any(point => point == null))
				{
					throw new NullReferenceException("point");
				}
				Progress.NewProgress("Распределение удаленных точек");
				Merger.MergePointsWithSmears(_smears, defectedPixels);

				Log.Trace("Обработка изображения завершена");
				Progress.NewProgress("Готово");
				Log.Trace($"Сформировано {clusters.Count} кластеров, {segmentsCount} сегментов, {smearsCount} мазков");
			});
			
		}

		private List<ImageView> CreateViews()
		{
			return new List<ImageView>
			{
				new ImageView(Model.Image, "Оригинал"),
				new ImageView(SuperPixels(), "Суперпиксели"),
				new ImageView(Clusters(), "Кластеры"),
				new ImageView(Segments(), "Сегменты"),
				new ImageView(BrushStrokes(), "Мазки"),
				new ImageView(RandomBrushStrokes(), "Мазки(рандом)"),
				new ImageView(BrushStrokesLines(), "Мазки(линии)")
			};
		}

		private BitmapSource SuperPixels()
		{
			Progress.NewProgress("Вычисление суперпикселей");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					var color = Utils.GetGandomData(3);
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(color.ToArray()));
					//obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(obj.Centroid.Pixels[Layers.Original].Data));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource Clusters()
		{
			Progress.NewProgress("Вычисление кластеров");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(smear.Cluster.Centroid.Data));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource Segments()
		{
			Progress.NewProgress("Вычисление сегментов");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(obj.Centroid.Pixels[Layers.Original].Data));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource BrushStrokes()
		{
			Progress.NewProgress("Вычисление мазков");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var center = objs.OrderBy(p => p.Centroid.Pixels[Layers.Original].Sum).ToList()[objs.Count / 2].Centroid.Pixels[Layers.Original].Data;
				foreach (var obj in objs)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(center));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource RandomBrushStrokes()
		{
			Progress.NewProgress("Вычисление мазков(рандом)");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var color = Utils.GetGandomData(3);
				color.Add(255);
				foreach (var obj in objs)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(color.ToArray()));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource BrushStrokesLines()
		{
			Progress.NewProgress("Вычисление мазков (линии)");
			return ImageHelper.PaintStrokes(Model.Image, _smears.Select(s => s.BrushStroke), ((int)_settings.MinSizeSuperpixel.Value) / 20 + 1);
		}

		public override string GetPlt()
		{
			return PltHelper.GetPlt(_smears.Select(s=>s.BrushStroke).ToList(), _settings.HeightPlt.Value, _settings.WidthPlt.Value, _settings.WidthSmear.Value, Model.Height, Model.Width);
		}
	}
}
