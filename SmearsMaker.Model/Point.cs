using System;

namespace SmearsMaker.Model
{
	public class Point
	{
		public System.Windows.Point Position { get; set; }
		public PixelCollection Pixels { get; }

		public Point(double posX, double posY)
		{
			Pixels = new PixelCollection();
			Position = new System.Windows.Point(posX, posY);
		}

		public Point(Point point)
		{
			Pixels = new PixelCollection(point.Pixels);

			Position = point.Position;
		}
	}
}
