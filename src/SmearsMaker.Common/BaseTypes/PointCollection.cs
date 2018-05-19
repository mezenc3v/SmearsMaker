using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SmearsMaker.Common.Helpers;

namespace SmearsMaker.Common.BaseTypes
{
	public class PointCollection : IEnumerable<Point>
	{
		internal event PropertyChangedEventHandler PropertyChanged;

		private readonly ObservableCollection<Point> _points;
		internal readonly List<string> Layers;

		public PointCollection()
		{
			_points = new ObservableCollection<Point>();
			_points.CollectionChanged += CollectionChanged;
			Layers = new List<string>();
		}

		internal PointCollection(PointCollection collection) : this()
		{
			Layers.AddRange(collection.Layers);
			foreach (var p in collection._points)
			{
				_points.Add(new Point(p));
			}
		}

		public PointCollection Clone()
		{
			return new PointCollection(this);
		}

		public void AddLayers(IEnumerable<string> layers)
		{
			if (layers == null)
			{
				throw new NullReferenceException(nameof(layers));
			}

			foreach (var layer in layers)
			{
				Addlayer(layer);
			}
		}

		public void Addlayer(string layer)
		{
			if (Layers.Contains(layer))
				throw new ArgumentException(nameof(layer));

			Layers.Add(layer);
			foreach (var point in _points)
			{
				point.AddLayer(layer);
			}
		}

		public void AddRange(PointCollection points)
		{
			AddRange(points.ToList());
		}

		public void AddRange(List<Point> points)
		{
			foreach (var point in points)
			{
				Add(point);
			}
		}

		public void Add(Point point)
		{
			_points.Add(point);
		}

		public int Count => _points.Count;

		public void Clear()
		{
			_points.Clear();
			Layers.Clear();
		}

		public Point this[int index]
		{
			get => _points[index];
			set => _points[index] = value;
		}

		public void ForEach(Action<Point> action)
		{
			_points.ForEach(action);
		}

		public IEnumerator<Point> GetEnumerator() => _points.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void CollectionChanged(object obj, NotifyCollectionChangedEventArgs args)
		{
			PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(nameof(_points)));
		}
	}
}