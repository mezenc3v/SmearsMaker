using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmearsMaker.HPGL
{
	internal static class PltHelper
	{
		internal static BitmapSource ToBitmapSource(DrawingImage source, int width, int heigth)
		{
			var drawingVisual = new DrawingVisual();
			var drawingContext = drawingVisual.RenderOpen();
			var bmp = new RenderTargetBitmap(width, heigth, 96, 96, PixelFormats.Pbgra32);
			drawingContext.DrawImage(source, new Rect(new Point(0, 0), new Size(width, heigth)));
			drawingContext.Close();
			bmp.Render(drawingVisual);
			return bmp;
		}
	}
}