using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmearsMaker.HPGL
{
	internal static class PltHelper
	{
		internal static Bitmap CreateBitmap(int width, int height)
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