using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace SmearsMaker.HPGL
{
	public class PltReader
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		private static readonly string _pattern = @"PC\d*,(?<Color>(\d*,*)*);(PW(?<Height>\d*),\d*;(PU[\d,]*;(PD[\d,]*;)*)*)*";
		private static readonly string _smearPattern = @"PU(?<Start>[\d,]*);(PD(?<Point>[\d,]*);)*";
		private static readonly string _positionsPattern = @"(PU|PD)(?<Position>\d*,\d*)";

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

			var bitmap = CreateBitmap(width, height);
			var g = Graphics.FromImage(bitmap);
			g.Clear(Color.White);

			foreach (Match match in matches)
			{
				try
				{
					var smearHeight = Convert.ToInt32(match.Groups["Height"].Value);
					var strColor = match.Groups["Color"].Value;
					var colorArr = strColor.Split(',');
					var color = colorArr.Select(p => Convert.ToByte(p)).ToArray();

					var pen = new Pen(Color.FromArgb(color[0], color[1], color[2]), smearHeight);
					var brush = new SolidBrush(Color.FromArgb(color[0], color[1], color[2]));

					var smearsMatches = Regex.Matches(match.Value, _smearPattern);

					foreach (Match smearMatch in smearsMatches)
					{
						var points = new List<PointF>();

						var startPointStr = smearMatch.Groups["Start"].Value;
						var startArr = startPointStr.Split(',');
						var startPointArr = startArr.Select(Convert.ToSingle).ToArray();
						var startPoint = new PointF(startPointArr[0], height - startPointArr[1]);
						points.Add(startPoint);

						var group = smearMatch.Groups["Point"];

						foreach (Capture capture in group.Captures)
						{
							var captureArr = capture.Value.Split(',');
							var pointArr = captureArr.Select(Convert.ToSingle).ToArray();
							var point = new PointF(pointArr[0], height - pointArr[1]);
							points.Add(point);
						}

						if (points.Count > 1)
						{
							g.FillEllipse(brush, points.First().X, points.First().Y, pen.Width, pen.Width);
							g.FillEllipse(brush, points.Last().X, points.Last().Y, pen.Width, pen.Width);

							g.DrawLines(pen, points.ToArray());
						}

						if (points.Count == 1)
						{
							g.FillEllipse(brush, points.First().X, points.First().Y, pen.Width, pen.Width);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Ошибка анализа plt файла");
					throw new Exception($"Ошибка анализа plt файла!\n{ex.Message}", ex);
				}
			}

			g.Dispose();

			return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());
		}

		private static Bitmap CreateBitmap(int width, int height)
		{
			using (var outStream = new MemoryStream())
			{
				var enc = new BmpBitmapEncoder();

				// Try creating a new image with a custom palette.
				var colors = new List<System.Windows.Media.Color>
				{
					Colors.Red,
					Colors.Blue,
					Colors.Green
				};

				var pf = PixelFormats.Rgb24;
				int rawStride = (width * pf.BitsPerPixel + 7) / 8;
				var pixels = new byte[rawStride * height];
				var myPalette = new BitmapPalette(colors);

				enc.Frames.Add(BitmapFrame.Create(BitmapSource.Create(width, height, 96, 96, pf, myPalette, pixels, rawStride)));
				enc.Save(outStream);
				return new Bitmap(outStream);
			}
		}
	}
}
