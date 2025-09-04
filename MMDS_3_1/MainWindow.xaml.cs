using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMDS_3_1
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public void PlotHeight(IList<double> time, IList<double> height,
							   string yLabel = "H, м", string xLabel = "t, с")
		{
			if (time == null || height == null || time.Count == 0 || time.Count != height.Count)
			{
				MessageBox.Show("Пустые данные или разная длина time/h.");
				return;
			}

			double w = PlotCanvas.ActualWidth > 0 ? PlotCanvas.ActualWidth : PlotCanvas.RenderSize.Width;
			double h = PlotCanvas.ActualHeight > 0 ? PlotCanvas.ActualHeight : PlotCanvas.RenderSize.Height;

			if (w <= 0 || h <= 0)
			{
				// окно ещё не измерено — перерисуем позже
				Dispatcher.InvokeAsync(() => PlotHeight(time, height, yLabel, xLabel));
				return;
			}

			const double leftPad = 60, rightPad = 20, topPad = 20, bottomPad = 40;
			double plotW = Math.Max(1, w - leftPad - rightPad);
			double plotH = Math.Max(1, h - topPad - bottomPad);

			double tMin = time.First();
			double tMax = time.Last();
			double yMin = height.Min();
			double yMax = height.Max();
			if (Math.Abs(tMax - tMin) < 1e-9) tMax = tMin + 1.0;
			if (Math.Abs(yMax - yMin) < 1e-9) yMax = yMin + 1.0;

			var points = new PointCollection(time.Count);
			for (int i = 0; i < time.Count; i++)
			{
				double tx = (time[i] - tMin) / (tMax - tMin);
				double ty = (height[i] - yMin) / (yMax - yMin);

				double x = leftPad + tx * plotW;
				double y = topPad + (1 - ty) * plotH;

				points.Add(new Point(x, y));
			}
			HeightPolyline.Points = points;

			DrawAxes(AxesCanvas, leftPad, topPad, plotW, plotH, tMin, tMax, yMin, yMax, xLabel, yLabel);
		}

		private void DrawAxes(Canvas canvas, double left, double top, double width, double height,
							  double tMin, double tMax, double yMin, double yMax,
							  string xLabel, string yLabel)
		{
			canvas.Children.Clear();

			var border = new Rectangle
			{
				Width = width,
				Height = height,
				Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
				StrokeThickness = 1
			};
			Canvas.SetLeft(border, left);
			Canvas.SetTop(border, top);
			canvas.Children.Add(border);

			int xTicks = 6, yTicks = 5;
			var gridBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
			var textBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));

			for (int i = 0; i <= xTicks; i++)
			{
				double k = (double)i / xTicks;
				double x = left + k * width;

				canvas.Children.Add(new Line { X1 = x, X2 = x, Y1 = top, Y2 = top + height, Stroke = gridBrush, StrokeThickness = i == 0 ? 1.5 : 1 });

				double tVal = tMin + k * (tMax - tMin);
				var label = new System.Windows.Controls.TextBlock { Text = tVal.ToString("0.#"), Foreground = textBrush, Margin = new Thickness(0, 4, 0, 0) };
				Canvas.SetLeft(label, x - 12);
				Canvas.SetTop(label, top + height + 4);
				canvas.Children.Add(label);
			}

			for (int j = 0; j <= yTicks; j++)
			{
				double k = (double)j / yTicks;
				double y = top + (1 - k) * height;

				canvas.Children.Add(new Line { X1 = left, X2 = left + width, Y1 = y, Y2 = y, Stroke = gridBrush, StrokeThickness = j == 0 ? 1.5 : 1 });

				double yVal = yMin + k * (yMax - yMin);
				var label = new System.Windows.Controls.TextBlock { Text = yVal.ToString("0.#"), Foreground = textBrush };
				Canvas.SetLeft(label, 8);
				Canvas.SetTop(label, y - 10);
				canvas.Children.Add(label);
			}

			var xLbl = new System.Windows.Controls.TextBlock { Text = xLabel, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
			Canvas.SetLeft(xLbl, left + width - 20);
			Canvas.SetTop(xLbl, top + height + 22);
			canvas.Children.Add(xLbl);

			var yLbl = new System.Windows.Controls.TextBlock { Text = yLabel, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
			Canvas.SetLeft(yLbl, 8);
			Canvas.SetTop(yLbl, top - 4);
			canvas.Children.Add(yLbl);
		}
	}
}
