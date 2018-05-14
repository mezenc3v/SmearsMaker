using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.Tracers.Helpers;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace SmearsMaker.Tracers.SmearTracer
{
	public class STracer : TracerBase
	{
		public override List<ImageSetting> Settings => _settings.Settings;
		public override List<ImageView> Views => CreateViews();

		private readonly StImageSettings _settings;
		private readonly IServicesFactory _factory;
		private List<Smear> _smears;

		public STracer(BitmapSource image) : base(image)
		{
			_settings = new StImageSettings(Model.Width, Model.Height);
			_factory = new SmearFactory(_settings, Model, Progress);
		}

		public override Task Execute()
		{
			var filter = _factory.CreateFilter();
			var kmeans = _factory.CreateClusterizer();
			var supPixSplitter = _factory.CreateSplitter();
			var bsm = _factory.CreateStrokesBuilder();

			var segmentsCount = 0;
			var smearsCount = 0;

			return Task.Run(() =>
			{
				_smears = new List<Smear>();
				Log.Trace("Начало обработки изображения");
				Progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				var filteredPoints = filter.Filtering(Model.Points);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				Progress.NewProgress("Кластеризация");
				var clusters = kmeans.Clustering(filteredPoints);
				Log.Trace($"Кластеризация заняла {sw.Elapsed.Seconds} с.");
				var defectedPixels = new List<Point>();
				Progress.NewProgress("Обработка", 0, clusters.Sum(c => c.Points.Count));
				Parallel.ForEach(clusters, (cluster) =>
				{
					var swClusters = Stopwatch.StartNew();
					var segments = SegmentSplitter.Split(cluster);

					segmentsCount += segments.Count;
					Log.Trace($"Сегментация кластера размером {cluster.Points.Count} пикселей заняла {swClusters.Elapsed.Seconds} с.");
					swClusters.Reset();
					Parallel.ForEach(segments, segment =>
					{
						var superPixels = supPixSplitter.Splitting(segment);

						Parallel.ForEach(superPixels, (supPix) =>
						{
							if (supPix.Points.Count < _settings.MinSizeSuperpixel.Value)
							{
								lock (defectedPixels)
								{
									defectedPixels.AddRange(supPix.Points);
								}
							}
						});

						if (superPixels.Count > 0)
						{
							var smears = bsm.Execute(superPixels.ToList<BaseShape>());
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
								Progress.Update(newSmear.BrushStroke.Objects.Sum(o => o.Points.Count));
							}
						}
					});
					Log.Trace($"Разбиение на мазки кластера размером {cluster.Points.Count} пикселей заняло {swClusters.Elapsed.Seconds} с.");
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
			var data = new PointCollection();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					var color = Utils.GetGandomData(3);
					obj.Points.ForEach(d => d.Pixels[Layers.Filtered] = Pixel.CreateInstance(color.ToArray()));
					//obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(obj.Centroid.Pixels[Layers.Original].Data));
					data.AddRange(obj.Points);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource Clusters()
		{
			Progress.NewProgress("Вычисление кластеров");
			var data = new PointCollection();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Points.ForEach(d => d.Pixels[Layers.Filtered] = Pixel.CreateInstance(smear.Cluster.Centroid));
					data.AddRange(obj.Points);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource Segments()
		{
			Progress.NewProgress("Вычисление сегментов");
			var data = new PointCollection();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Points.ForEach(d => d.Pixels[Layers.Filtered] = Pixel.CreateInstance(obj.GetCenter(Layers.Original).Data));
					data.AddRange(obj.Points);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource BrushStrokes()
		{
			Progress.NewProgress("Вычисление мазков");
			var data = new PointCollection();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var center = objs.OrderBy(p => p.GetCenter(Layers.Original).Sum).ToList()[objs.Count / 2].GetCenter(Layers.Original).Data;
				foreach (var obj in objs)
				{
					obj.Points.ForEach(d => d.Pixels[Layers.Filtered] = Pixel.CreateInstance(center));
					data.AddRange(obj.Points);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource RandomBrushStrokes()
		{
			Progress.NewProgress("Вычисление мазков(рандом)");
			var data = new PointCollection();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var color = Utils.GetGandomData(3);
				color.Add(255);
				foreach (var obj in objs)
				{
					obj.Points.ForEach(d => d.Pixels[Layers.Filtered] = Pixel.CreateInstance(color.ToArray()));
					data.AddRange(obj.Points);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource BrushStrokesLines()
		{
			Progress.NewProgress("Вычисление мазков (линии)");
			return ImageHelper.PaintStrokes(Model.Image, _smears.Select(s => s.BrushStroke), ((int)_settings.MinSizeSuperpixel.Value) / 20 + 1);
		}

		public override string CreatePlt()
		{
			return PltHelper.GetPlt(_smears.Select(s=>s.BrushStroke).ToList(), _settings.HeightPlt.Value, _settings.WidthPlt.Value, _settings.WidthSmear.Value, Model.Height, Model.Width);
		}

		public override void Dispose()
		{
			_smears?.Clear();
			base.Dispose();
		}
	}
}
