using System;

namespace SmearTracer.Model
{
	public class Point
	{
		public System.Windows.Point Position { get; }
		public Pixel Original { get; }
		public Pixel Filtered { get; private set; }

		public Point(Pixel original, double posX, double posY)
		{
			Position = new System.Windows.Point(posX, posY);
			Original = original ?? throw new NullReferenceException("original");
		}

		public Point(Point point)
		{
			Original = new Pixel(point.Original);
			if (point.Filtered != null)
			{
				Filtered = new Pixel(point.Filtered);
			}

			Position = point.Position;
		}

		public void SetFilteredPixel(float[] filtered)
		{
			Filtered = new Pixel(filtered);
		}
	}
}
