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

			// --- 1) Получаем данные из Core ---
			var aircraft = new AircraftGeometryAndInertia();
			var flight = new FlightAndAeroParams();
			var state = new AircraftState(aircraft);
			var coeff = new DiffEqCoefficientsCalculator();
			var controlLaw = new CalculateControlLaw(new ControlLawParams());
			var sim = new Simulation(coeff, controlLaw);

			// Настройки прогонки — как у тебя в примере
			double tEnd = 1000.0, dt = 0.01, tStartDropping = 2.0;
			double Hset = 600; int lawNum = 1; double aCargo = 0.3, lCabin = 100;

			SimulationResult result = sim.Run(
				aircraftParams: aircraft,
				flightParams: flight,
				state: state,
				tEnd: tEnd,
				dt: dt,
				tStartDropping: tStartDropping,
				hz: Hset,
				controlLawNumber: lawNum,
				aCargo: aCargo,
				lCabin: lCabin
			);

			// --- 2) Строим график в OxyPlot ---
			var model = new PlotModel
			{
				Title = "Изменение высоты H(t)",
				TextColor = OxyColors.White,
				PlotAreaBorderColor = OxyColor.FromRgb(90, 90, 90),
				Background = OxyColors.Transparent
			};

			// Ось X (время, с)
			var xAxis = new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "t, с",
				MajorGridlineStyle = LineStyle.Solid,
				MinorGridlineStyle = LineStyle.Dot,
				MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
				MinorGridlineColor = OxyColor.FromRgb(50, 50, 50),
				StringFormat = "0.0"
			};
			model.Axes.Add(xAxis);

			// Ось Y (высота, м)
			var yAxis = new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = "H, м",
				MajorGridlineStyle = LineStyle.Solid,
				MinorGridlineStyle = LineStyle.Dot,
				MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
				MinorGridlineColor = OxyColor.FromRgb(50, 50, 50),
				StringFormat = "0"
			};
			model.Axes.Add(yAxis);

			// Линия H(t)
			var series = new LineSeries
			{
				Title = "H(t)",
				Color = OxyColors.DeepSkyBlue,
				StrokeThickness = 2
			};

			// наполняем точками
			for (int i = 0; i < result.Time.Count; i++)
				series.Points.Add(new DataPoint(result.Time[i], result.H[i]));

			model.Series.Add(series);

			// Привязываем модель к контролу
			PlotView.Model = model;
		}
	}
}
