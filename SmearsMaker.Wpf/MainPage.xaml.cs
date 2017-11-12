using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using SmearsMaker.Logic;

namespace SmearsMaker.Wpf
{
	/// <summary>
	/// Interaction logic for MainPage.xaml
	/// </summary>
	public partial class MainPage : Page
	{
		private readonly ApplicationViewModel _model;

		public MainPage()
		{
			InitializeComponent();
			_model = new ApplicationViewModel();
			DataContext = _model;
		}

		private async void ButtonRun_Click(object sender, RoutedEventArgs e)
		{
			await _model.Run();
		}

		private void Page_KeyUp(object sender, KeyEventArgs e)
		{
			_model.ChangeImage(e);
		}
	}
}
