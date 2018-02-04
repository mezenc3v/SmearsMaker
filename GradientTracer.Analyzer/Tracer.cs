using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using SmearsMaker.Common;
using SmearsMaker.Model;
using SmearsMaker.Filtering;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using GradientTracer.FeatureDetection;
using SmearTracer.Concatenation;
using SmearTracer.Segmentation;
using SmearTracer.Segmentation.SimpleSegmentsSplitter;
using SmearTracer.Segmentation.SuperpixelSplitter;
using Consts = SmearsMaker.Model.Consts;
using Pen = System.Windows.Media.Pen;
using Point = SmearsMaker.Model.Point;

namespace GradientTracer.Analyzer
{
	public class Tracer : ITracer
	{
		public List<ImageSetting> Settings => _model.Settings;

		private readonly ImageModel _model;
		private List<Point> _sobelPoints;
		private SmearsMaker.Model.ImageModel _data;
		private readonly Progress _progress;
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		private List<SuperPixel> _superPixels;
		public List<BrushStroke> _smears;

		public Tracer(BitmapImage image, Progress progress)
		{
			_progress = progress ?? throw new NullReferenceException(nameof(progress));
			_model = new ImageModel(image);
		}

		public Task Execute()
		{
			_data = SmearsMaker.Model.ImageModel.ConvertBitmapToImage(_model.Image, ColorModel.Rgb);
			var filter = new MedianFilter((int)_model.FilterRank.Value, _model.Width, _model.Height);
			var sobel = new Sobel(_model.Width, _model.Height);
			var bsm = new BsmPair(Math.Sqrt(_model.SizeSuperPixel.Value) + 1, (float)_model.Tolerance.Value);
			var centroid = new Point(_model.Width / 2, _model.Height / 2);
			centroid.Pixels.AddPixel(Consts.Original, null);
			var segment = new Segment
			{
				Centroid = centroid,
				MaxX = new System.Windows.Point(_model.Width, _model.Height),
				MaxY = new System.Windows.Point(_model.Width, _model.Height),
				MinX = new System.Windows.Point(0, 0),
				MinY = new System.Windows.Point(0, 0)
			};

			return Task.Run(() =>
			{
				Log.Trace("Начало обработки изображения");
				_progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				filter.Filter(_data);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				_progress.NewProgress("Вычисление градиентов");
				_sobelPoints = sobel.Compute(_data);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");

				segment.Data = _sobelPoints;
				var splitter = new SuperpixelSplitter((int)_model.SizeSuperPixel.Value, (int)_model.SizeSuperPixel.Value, 1);
				_superPixels = splitter.Splitting(segment);

				foreach (var superPixel in _superPixels)
				{
					var averGrad = new float[4];
					var averData = new float[4];
					superPixel.Data.ForEach(d =>
					{
						for (int i = 0; i < averGrad.Length; i++)
						{
							averGrad[i] += d.Pixels[FeatureDetection.Consts.Gradient].Data[i];
							averData[i] += d.Pixels[SmearsMaker.Model.Consts.Original].Data[i];
						}
					});

					for (int i = 0; i < averGrad.Length; i++)
					{
						averGrad[i] /= superPixel.Data.Count;
						averData[i] /= superPixel.Data.Count;
					}

					superPixel.Data.ForEach(d =>
					{
						d.Pixels.AddPixel(FeatureDetection.Consts.SuperPixelsGrad, new Pixel(averGrad));
					});
					superPixel.Centroid.Pixels.AddPixel(FeatureDetection.Consts.SuperPixelsGrad, new Pixel(averGrad));
					superPixel.Centroid.Pixels.AddPixel(FeatureDetection.Consts.Gradient, new Pixel(averGrad));
					superPixel.Centroid.Pixels[Consts.Original] = new Pixel(averData);
				}
				_progress.NewProgress("Создание мазков");
				_smears = bsm.Execute(_superPixels.ToList<IObject>());

				Log.Trace("Обработка изображения завершена");
				_progress.NewProgress("Готово");
				//Log.Trace($"Сформировано {clusters.Count} кластеров, {segmentsCount} сегментов, {smearsCount} мазков");
			});
		}

		public List<ImageViewModel> Views => new List<ImageViewModel>
		{
			new ImageViewModel(_model.Image, "Оригинал"),
			new ImageViewModel(Antialiasing(), "Размытое изображение"),
			new ImageViewModel(SobelGradients(), "Поле градиентов"),
			new ImageViewModel(SobelCurves(), "Границы"),
			new ImageViewModel(SuperPixels(), "Суперпиксели"),
			new ImageViewModel(SuperPixelsGrad(), "Суперпиксели-градиенты"),
			new ImageViewModel(Smears(), "Мазки"),
		};
		public static List<float> GetGandomData(uint length)
		{
			var c = new RNGCryptoServiceProvider();
			var randomNumber = new byte[length];
			c.GetBytes(randomNumber);

			return randomNumber.Select(b => (float)b).ToList();
		}

		private BitmapSource SuperPixels()
		{
			_progress.NewProgress("Вычисление суперпикселей");
			var data = new List<Point>();
			foreach (var superPixel in _superPixels)
			{
				var rand = GetGandomData(3).ToArray();
				superPixel.Data.ForEach(d =>
				{
					d.Pixels.AddPixel(Consts.SuperPixels, new Pixel(rand));
				});
				data.AddRange(superPixel.Data);
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data, SmearsMaker.Model.Consts.SuperPixels);
		}

		private BitmapSource SuperPixelsGrad()
		{
			_progress.NewProgress("Вычисление градиентов суперпикселей");
			var data = new List<Point>();
			foreach (var superPixel in _superPixels)
			{
				data.AddRange(superPixel.Data);
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data, FeatureDetection.Consts.Gradient);
		}
		private BitmapSource Smears()
		{
			_progress.NewProgress("Вычисление мазков");
			_progress.NewProgress("Вычисление мазков (линии)");
			Bitmap bitmap;
			using (var outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(_model.Image));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}

			var g = Graphics.FromImage(bitmap);
			//g.Clear(System.Drawing.Color.White);

			foreach (var smear in _smears.OrderByDescending(s => s.Objects.Count))
			{
				var center = smear.Objects.OrderBy(p => p.Centroid.Pixels[Consts.Original].Sum).ToList()[smear.Objects.Count / 2].Centroid;

				var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb((byte)center.Pixels[Consts.Original].Data[3], (byte)center.Pixels[Consts.Original].Data[2],
					(byte)center.Pixels[Consts.Original].Data[1], (byte)center.Pixels[Consts.Original].Data[0]));

				var brush = new SolidBrush(System.Drawing.Color.FromArgb((byte)center.Pixels[Consts.Original].Data[3], (byte)center.Pixels[Consts.Original].Data[2],
					(byte)center.Pixels[Consts.Original].Data[1], (byte)center.Pixels[Consts.Original].Data[0]));
				//var random = GetGandomData(4);
				//var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb((byte)random[0], (byte)random[1],
				//	(byte)random[2], (byte)random[3]));

				//var brush = new SolidBrush(System.Drawing.Color.FromArgb((byte)random[0], (byte)random[1],
				//	(byte)random[2], (byte)random[3]));

				var pointsF = smear.Objects.Select(point => new PointF((int)point.Centroid.Position.X, (int)point.Centroid.Position.Y)).ToArray();
				pen.Width = (float)_model.HeightSmear.Value;
				if (pointsF.Length > 1)
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
					g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y, pen.Width, pen.Width);
					g.DrawLines(pen, pointsF);
				}
				else
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
				}
			}

			g.Dispose();

			var bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			return bmp;
		}


		private BitmapSource Antialiasing()
		{
			_progress.NewProgress("Вычисление размытия");
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, _data.Points.ToList(), SmearsMaker.Model.Consts.Filtered);
		}

		private BitmapSource SobelGradients()
		{
			_progress.NewProgress("Вычисление градиентов");
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, _sobelPoints, FeatureDetection.Consts.Gradient);
		}

		private BitmapSource SobelCurves()
		{
			_progress.NewProgress("Вычисление границ");
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, _sobelPoints, FeatureDetection.Consts.Curves);
		}

		public string GetPlt()
		{
			throw new NotImplementedException();
		}
	}
}
