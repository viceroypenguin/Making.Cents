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
using Making.Cents.Wpf.Common.ViewModels;

namespace Making.Cents.Wpf.Common.Views
{
	public interface IUsesLoadingViewModel
	{
		LoadingViewModel LoadingViewModel { get; }
	}

	/// <summary>
	/// Interaction logic for LoadingView.xaml
	/// </summary>
	public partial class LoadingView : UserControl
	{
		public LoadingView()
		{
			InitializeComponent();
		}
	}
}
