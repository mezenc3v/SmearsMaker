namespace SmearsMaker.Common.BaseTypes
{
	public class Point
	{
		public System.Windows.Point Position { get; }
		public PixelCollection Pixels { get; }

		internal Point(System.Windows.Point point)
		{
			Pixels = new PixelCollection();
			Position = point;
		}

		internal Point(double posX, double posY) : this(new System.Windows.Point(posX, posY))
		{
		}

		internal Point(Point point)
		{
			Pixels = new PixelCollection(point.Pixels);
			Position = point.Position;
		}

		internal void AddLayer(string layer)
		{
			if (!Pixels.Layers.Contains(layer))
			{
				Pixels.Addlayer(layer);
			}
		}

		public Point Clone()
		{
			return new Point(this);
		}
	}
}
