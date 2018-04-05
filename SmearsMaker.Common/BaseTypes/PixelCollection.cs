using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmearsMaker.Common.BaseTypes
{
	public class PixelCollection : IEnumerable<Pixel>
	{
		private readonly Dictionary<string, Pixel> _pixels;

		internal List<string> Layers => _pixels.Keys.ToList();

		internal PixelCollection()
		{
			_pixels = new Dictionary<string, Pixel>();
		}

		internal PixelCollection(PixelCollection collection)
		{
			_pixels = new Dictionary<string, Pixel>();
			foreach (var p in collection._pixels)
			{
				var newPixel = new Pixel(p.Value.Data);
				_pixels.Add(p.Key, newPixel);
			}
		}

		internal void Addlayer(string layer)
		{
			_pixels.Add(layer, new Pixel());
		}

		internal void AddPixel(string layer, Pixel value)
		{
			_pixels.Add(layer, value);
		}
		public Pixel this[string index]
		{
			get => _pixels[index];
			set => _pixels[index] = new Pixel(value);
		}

		public IEnumerator<Pixel> GetEnumerator() => _pixels.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}