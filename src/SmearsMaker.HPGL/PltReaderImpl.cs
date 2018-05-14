using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SmearsMaker.HPGL
{
	public class PltReaderImpl : IPltReader
	{
		private readonly Painter _painter;
		private static readonly string _pattern = @"PW(?<Width>\d*),\d*;(PC\d*,(?<Color>(\d*,*)*);(PU[\d,]*;(PD[\d,]*);*)*)*";
		private static readonly string _smearPattern = @"PC\d*,(?<Color>(\d*,*)*);PU(?<Start>[\d,]*);PD(?<Points>[\d,]*);";
		private static readonly string _positionsPattern = @"(PD|PU)((?<Position>\d*,\d*)(,|;))*";

		public PltReaderImpl()
		{
			_painter = new Painter();
		}

		public BitmapSource ReadPlt(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException(nameof(path));
			}

			var file = File.ReadAllText(path);
			var matches = Regex.Matches(file, _pattern);

			var (width, height) = GetDimensions(file);

			var widthOfSmears = new List<int>();
			foreach (Match match in matches)
			{
				try
				{
					var smearwidth = Convert.ToInt32(match.Groups["Width"].Value);
					widthOfSmears.Add(smearwidth);
					var pens = Regex.Matches(match.Value, _smearPattern);
					foreach (Match smearPen in pens)
					{
						var colorArr = smearPen.Groups["Color"].Value.Split(',');
						var colorData = colorArr.Select(p => Convert.ToByte(p)).ToArray();

						_painter.SetPens(colorData, smearwidth);

						var points = new List<Point>();
						var startArr = smearPen.Groups["Start"].Value.Split(',');
						var startPointArr = startArr.Select(Convert.ToSingle).ToArray();
						var startPoint = new Point(startPointArr[0], height - startPointArr[1]);
						points.Add(startPoint);
						var pointsGroup = smearPen.Groups["Points"];

						foreach (Capture capture in pointsGroup.Captures)
						{
							var pointArr = capture.Value.Split(',').Select(Convert.ToSingle).ToArray();
							for (int i = 0; i < pointArr.Length; i += 2)
							{
								var point = new Point(pointArr[i], height - pointArr[i + 1]);
								points.Add(point);
							}
						}
						_painter.AddPoints(points);
					}
				}
				catch (Exception ex)
				{
					throw new Exception($"Ошибка анализа plt файла!\n{ex.Message}", ex);
				}
			}

			int averageWidth = (int)widthOfSmears.Average();
			return PltHelper.ToBitmapSource(_painter.Image, height / averageWidth * 10, width / averageWidth * 10);
		}

		private static (int width, int height) GetDimensions(string file)
		{
			int maxWidth = 0;
			int maxHeight = 0;
			int minWidth = int.MaxValue;
			int minHeight = int.MaxValue;

			foreach (Match position in Regex.Matches(file, _positionsPattern))
			{
				foreach (Capture capture in position.Groups["Position"].Captures)
				{
					var posArr = capture.Value.Split(',').Select(p => Convert.ToInt32(p)).ToArray();

					if (posArr[0] > maxWidth)
					{
						maxWidth = posArr[0];
					}
					if (posArr[1] > maxHeight)
					{
						maxHeight = posArr[1];
					}

					if (posArr[0] < minWidth)
					{
						minWidth = posArr[0];
					}
					if (posArr[1] < minHeight)
					{
						minHeight = posArr[1];
					}
				}
			}

			return (maxWidth - minWidth, maxHeight - minHeight);
		}
	}
}
