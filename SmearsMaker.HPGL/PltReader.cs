using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace SmearsMaker.HPGL
{
	public class PltReader
	{
		private static readonly string _pattern = @"PW(?<Width>\d*),\d*;(PC\d*,(?<Color>(\d*,*)*);(PU[\d,]*;(PD[\d,]*);*)*)*";
		private static readonly string _smearPattern = @"PC\d*,(?<Color>(\d*,*)*);PU(?<Start>[\d,]*);PD(?<Points>[\d,]*);";
		private static readonly string _positionsPattern = @"(PD|PU)((?<Position>\d*,\d*)(,|;))*";

		public PltReader()
		{

		}

		public BitmapSource Read(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException(nameof(path));
			}

			var file = File.ReadAllText(path);
			var matches = Regex.Matches(file, _pattern);

			var (width, height) = GetDimensions(file);

			var bitmap = PltHelper.CreateBitmap(width, height);
			var g = Graphics.FromImage(bitmap);
			g.Clear(Color.White);

			foreach (Match match in matches)
			{
				try
				{
					var smearwidth = Convert.ToInt32(match.Groups["Width"].Value);
					var pens = Regex.Matches(match.Value, _smearPattern);

					foreach (Match smearPen in pens)
					{
						var colorArr = smearPen.Groups["Color"].Value.Split(',');
						var color = colorArr.Select(p => Convert.ToByte(p)).ToArray();
						var pen = new Pen(Color.FromArgb(color[0], color[1], color[2]), smearwidth);
						var brush = new SolidBrush(Color.FromArgb(color[0], color[1], color[2]));

						var points = new List<PointF>();

						var startArr = smearPen.Groups["Start"].Value.Split(',');
						var startPointArr = startArr.Select(Convert.ToSingle).ToArray();
						var startPoint = new PointF(startPointArr[0], height - startPointArr[1]);
						points.Add(startPoint);

						var pointsGroup = smearPen.Groups["Points"];

						foreach (Capture capture in pointsGroup.Captures)
						{
							var pointArr = capture.Value.Split(',').Select(Convert.ToSingle).ToArray();
							for(int i = 0; i < pointArr.Length - 1; i+=2)
							{
								var point = new PointF(pointArr[i], height - pointArr[i + 1]);
								points.Add(point);
							}						
						}

						g.FillEllipse(brush, points.First().X, points.First().Y, pen.Width, pen.Width);

						if (points.Count > 1)
						{
							g.DrawLines(pen, points.ToArray());
							g.FillEllipse(brush, points.Last().X, points.Last().Y, pen.Width, pen.Width);
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception($"Ошибка анализа plt файла!\n{ex.Message}", ex);
				}
			}

			g.Dispose();

			return Imaging.CreateBitmapSourceFromHBitmap(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());
		}

		private static (int width, int height) GetDimensions(string file)
		{
			int width = 0;
			int height = 0;

			foreach (Match position in Regex.Matches(file, _positionsPattern))
			{
				foreach (Capture capture in position.Groups["Position"].Captures)
				{
					var posArr = capture.Value.Split(',').Select(p => Convert.ToInt32(p)).ToArray();

					if (posArr[0] > width)
					{
						width = posArr[0];
					}
					if (posArr[1] > height)
					{
						height = posArr[1];
					}
				}
			}

			return (width, height);
		}
	}
}
