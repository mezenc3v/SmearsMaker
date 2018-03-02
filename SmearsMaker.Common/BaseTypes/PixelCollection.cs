using System.Collections;
using System.Collections.Generic;

namespace SmearsMaker.Common.BaseTypes
{
	public class PixelCollection : IEnumerable
	{
		private readonly Dictionary<string, Pixel> _pixels;

		public PixelCollection()
		{
			_pixels = new Dictionary<string, Pixel>();
		}

		public PixelCollection(PixelCollection collection)
		{
			_pixels = new Dictionary<string, Pixel>();
			foreach (var p in collection._pixels)
			{
				_pixels.Add(p.Key, p.Value);
			}
		}

		public void AddPixel(string layer, Pixel value)
		{
			_pixels.Add(layer, value);
		}
		public Pixel this[string index]
		{
			get => _pixels[index];
			set => _pixels[index] = value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _pixels.GetEnumerator();
		}
	}
}