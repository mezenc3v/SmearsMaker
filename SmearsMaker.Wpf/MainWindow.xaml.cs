using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SmearsMaker.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly ApplicationViewModel _model;

		public MainWindow()
		{
			InitializeComponent();
			_model = new ApplicationViewModel();
			DataContext = _model;
			Algorithms.ItemsSource = _model.Tracers;
		}

		private async void ButtonRun_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await _model.Run();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			_model.ChangeImage(e);
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var settingsForm = new SettingsWindow(_model.Settings);
			settingsForm.Show();
		}

		private void TracerMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var tracer = (e.OriginalSource as MenuItem)?.DataContext as Type;
			_model.SetAlgorithm(tracer);
		}

	private void ClipboardCopyMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (Image.Source != null)
			{
				Clipboard.SetImage((BitmapSource)Image.Source);
			}
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_model.OpenPlt();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}

