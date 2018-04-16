using System;
using SmearsMaker.Common;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing;
using SmearsMaker.ImageProcessing.FeatureDetection;
using SmearsMaker.ImageProcessing.Filtering;
using SmearsMaker.Tracers.Logic;

namespace SmearsMaker.Tracers.GradientTracers
{
	public class SquadTracerFactory : IServicesFactory
	{
		private readonly ImageModel _model;
		private readonly IProgress _progress;
		private readonly GtImageSettings _settings;

		public SquadTracerFactory(GtImageSettings settings, ImageModel model, IProgress progress)
		{
			_model = model;
			_progress = progress;
			_settings = settings;
		}

		public IStrokesBuilder CreateStrokesBuilder()
		{
			return new GradientStrokesBuilder(_progress, _settings.WidthSmear.Value * 2 - 2, (float)_settings.Tolerance.Value, (float)_settings.Tolerance2.Value);
		}

		public ISplitter CreateSplitter()
		{
			return new SuperpixelSplitter(_progress, (int)_settings.WidthSmear.Value);
		}

		public IClusterizer CreateClusterizer()
		{
			throw new NotImplementedException();
		}

		public IFilter CreateFilter()
		{
			return new MedianFilter((int)_settings.FilterRank.Value, _model.Width, _model.Height);
		}

		public IDetector CreateDetector()
		{
			return new Sobel(_model.Width, _model.Height);
		}
	}
}