using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Coefficient;
using Core.Params;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace UI;

public class MainWindowViewModel : INotifyPropertyChanged
{
	public PlotModel HeightPlot { get; set; }
	public PlotModel DvPlot { get; set; }
	public PlotModel AlphaPlot { get; set; }
	public PlotModel ThetaPlot { get; set; }

	public PlotModel AlphaBal { get; set; }
	public PlotModel DeltaVBal { get; set; }

	public ICommand RefreshAllCommand { get; }

	public MainWindowViewModel()
	{
		RefreshAllCommand = new RelayCommand(RefreshAll);

		// --- 1) Получаем данные из Core ---
		var aircraft = new AircraftGeometryAndInertia();
		var flight = new FlightAndAeroParams();
		var state = new AircraftState(aircraft);
		var coeff = new DiffEqCoefficientsCalculator();
		var controlLaw = new CalculateControlLaw(new ControlLawParams());
		var sim = new Simulation(coeff, controlLaw);

		// Настройки прогонки — как у тебя в примере
		double tEnd = 2000, dt = 0.01, tStartDropping = 200;
		double Hset = 600; int lawNum = 5; double aCargo = 0.3, lCabin = 100;

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


		HeightPlot = BuildPlotModel(
			"Изменение высоты H(t)",
			"t, с", "0.0",
			"H, м", "0",
			"H(t)",
			result.Time.Zip(result.H, (t, h) => new DataPoint(t, h)).ToList()
		);
		DvPlot = BuildPlotModel(
			"Изменения руля высоты DV(t)",
			"t, с", "0.0",
			"ΔV, °", "0.0",
			"DV(t)",
			result.Time.Zip(result.Dv, (t, h) => new DataPoint(t, h)).ToList()
		);
		AlphaPlot = BuildPlotModel(
			"Изменение угла атаки Alpha(t)",
			"t, с", "0.0",
			"α, °", "0.0",
			"Alpha(t)",
			result.Time.Zip(result.Alpha, (t, h) => new DataPoint(t, h)).ToList()
		);
		ThetaPlot = BuildPlotModel(
			"Изменение угла тангажа Theta(t)",
			"t, с", "0.0",
			"θ, °", "0.0",
			"Theta(t)",
			result.Time.Zip(result.Theta, (t, h) => new DataPoint(t, h)).ToList()
		);
		AlphaBal = BuildPlotModel(
			"Изменение балансированного угла атаки",
			"t, с", "0.0",
			"α, °", "0.0",
			"Theta(t)",
			result.Time.Zip(result.AlphaBal, (t, h) => new DataPoint(t, h)).ToList()
		);
		DeltaVBal = BuildPlotModel(
			"Изменение балансированого значения руля высоты",
			"t, с", "0.0",
			"θ, °", "0.0",
			"Theta(t)",
			result.Time.Zip(result.DeltaVBal, (t, h) => new DataPoint(t, h)).ToList()
		);
	}

	private void RefreshAll()
	{
		// If you mutate existing models, just InvalidatePlot
		Invalidate(HeightPlot);
		Invalidate(DvPlot);
		Invalidate(AlphaPlot);
		Invalidate(ThetaPlot);
	}
	private static void Invalidate(PlotModel m) => m?.InvalidatePlot(true);

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	private static PlotModel BuildPlotModel(
		string title,
		string xAxisTitle, string xAxisFormat,
		string yAxisTitle, string yAxisFormat,
		string seriesTitle,
		List<DataPoint> dataPoints)
	{
		var model = new PlotModel
		{
			Title = title,
			TextColor = OxyColors.White,
			PlotAreaBorderColor = OxyColor.FromRgb(90, 90, 90),
			Background = OxyColors.Transparent
		};

		var xAxis = new LinearAxis
		{
			Position = AxisPosition.Bottom,
			Title = xAxisTitle,
			MajorGridlineStyle = LineStyle.Solid,
			MinorGridlineStyle = LineStyle.Dot,
			MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
			MinorGridlineColor = OxyColor.FromRgb(50, 50, 50),
			StringFormat = xAxisFormat
		};
		model.Axes.Add(xAxis);
		var yAxis = new LinearAxis
		{
			Position = AxisPosition.Left,
			Title = yAxisTitle,
			MajorGridlineStyle = LineStyle.Solid,
			MinorGridlineStyle = LineStyle.Dot,
			MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
			MinorGridlineColor = OxyColor.FromRgb(50, 50, 50),
			StringFormat = yAxisFormat
		};
		model.Axes.Add(yAxis);

		var series = new LineSeries
		{
			Title = seriesTitle,
			Color = OxyColors.DeepSkyBlue,
			StrokeThickness = 2
		};
		series.Points.AddRange(dataPoints);
		model.Series.Add(series);

		return model;
	}
}
