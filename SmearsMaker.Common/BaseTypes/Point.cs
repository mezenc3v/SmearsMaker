namespace SmearsMaker.Common.BaseTypes
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

		private Point(Point point)
		{
			Pixels = new PixelCollection(point.Pixels);
			Position = point.Position;
		}

		public Point Clone()
		{
			return new Point(this);
		}
	}
}
