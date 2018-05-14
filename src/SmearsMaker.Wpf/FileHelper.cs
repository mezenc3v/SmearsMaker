using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SmearsMaker.Common;

namespace SmearsMaker.Wpf
{
	public static class FileHelper
	{
		internal static List<Type> LoadLibraries()
		{
			var libs = new List<Type>();
			var tracerLibs = ConfigurationManager.AppSettings;
			foreach (var tracerLib in tracerLibs.AllKeys)
			{
				var value = tracerLibs.Get(tracerLib);
				var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				if (assemblyFolder == null) continue;
				var assemblyPath = Path.Combine(assemblyFolder, value);
				if (!File.Exists(assemblyPath)) continue;
				var assembly = Assembly.LoadFrom(assemblyPath);
				var tracers = assembly.GetTypes().Where(t => typeof(ITracer).IsAssignableFrom(t) && !t.IsAbstract).ToList();
				libs.AddRange(tracers);
			}
			return libs;
		}

		internal static string GetFolder()
		{
			var sf = new SaveFileDialog
			{
				FileName = "select folder"
			};

			return sf.ShowDialog() != true ? null : Path.GetDirectoryName(sf.FileName);
		}

		internal static void SaveBitmapSource(string path, string name, BitmapSource source)
		{
			if (path == null) return;
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(source));
			using (var filestream = new FileStream(Path.Combine(path, $"{name}.bmp"), FileMode.Create))
			{
				encoder.Save(filestream);
			}
		}

		internal static Uri OpenImage()
		{
			//считывание с файла
			var fileDialog = new OpenFileDialog
			{
				Filter =
					"JPG Files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)" +
					"|*.png|GIF Files (*.gif)|*.gif|All files (*.*)|*.*",
				RestoreDirectory = true
			};
			return fileDialog.ShowDialog() == true ? new Uri(fileDialog.FileName) : null;
		}

		internal static string OpenPlt()
		{
			//считывание с файла
			var fileDialog = new OpenFileDialog
			{
				Filter =
					"Plt Files (*.plt)|*.plt|All files (*.*)|*.*",
				RestoreDirectory = true
			};
			return fileDialog.ShowDialog() != true ? null : fileDialog.FileName;
		}

		internal static void SavePlt(string plt)
		{
			var fileDialog = new SaveFileDialog
			{
				FileName = "pltFile",
				DefaultExt = ".plt",
				Filter = "Plt Files (*.plt)|*.plt|All files (*.*)|*.*",
				RestoreDirectory = true
			};

			if (fileDialog.ShowDialog() == true)
			{
				File.WriteAllText(fileDialog.FileName, plt, Encoding.ASCII);
			}
		}
	}
}