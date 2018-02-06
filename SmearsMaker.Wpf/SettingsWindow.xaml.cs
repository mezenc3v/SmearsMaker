using System.Collections.Generic;
using System.Windows;
using SmearsMaker.Common.Image;

namespace SmearsMaker.Wpf
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow(IEnumerable<ImageSetting> settings)
		{
			InitializeComponent();

			lstSettings.ItemsSource = settings;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
