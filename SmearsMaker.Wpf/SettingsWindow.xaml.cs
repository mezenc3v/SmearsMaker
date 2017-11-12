﻿using System;
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
using System.Windows.Shapes;

namespace SmearsMaker.Wpf
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		private readonly SettingsViewModel _model;
		public SettingsWindow(SettingsViewModel model)
		{
			InitializeComponent();

			model = model ?? new SettingsViewModel()
			{
				ClusterMaxIteration = 5
			};
			_model = model;
			DataContext = _model;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
