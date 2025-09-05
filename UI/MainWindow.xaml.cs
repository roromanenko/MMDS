using System.Linq;
using System.Windows;
using Core;
using Core.Params;
using Core.Coefficient;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace UI
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainWindowViewModel();
		}
	}
}
