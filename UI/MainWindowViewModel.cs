using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Coefficient;
using Core.Params;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace UI;

public class MainWindowViewModel : INotifyPropertyChanged
{
	private double _tEnd = 50;
	public double TEnd { get => _tEnd; set => SetProperty(ref _tEnd, value); }

	private double _dt = 0.01;
	public double Dt { get => _dt; set => SetProperty(ref _dt, value); }

	private double _tStartDropping = 5;
	public double TStartDropping { get => _tStartDropping; set => SetProperty(ref _tStartDropping, value); }

	private double _hSet = 600;
	public double HSet { get => _hSet; set => SetProperty(ref _hSet, value); }

	private int _lawNum = 5;
	public int LawNum { get => _lawNum; set => SetProperty(ref _lawNum, value); }

	private double _aCargo = 0.3;
	public double ACargo { get => _aCargo; set => SetProperty(ref _aCargo, value); }

	private double _lCabin = 100;
	public double LCabin { get => _lCabin; set => SetProperty(ref _lCabin, value); }

	private PlotModel _heightPlot;
	public PlotModel HeightPlot { get => _heightPlot; private set => SetProperty(ref _heightPlot, value); }

	private PlotModel _dvPlot;
	public PlotModel DvPlot { get => _dvPlot; private set => SetProperty(ref _dvPlot, value); }

	private PlotModel _alphaPlot;
	public PlotModel AlphaPlot { get => _alphaPlot; private set => SetProperty(ref _alphaPlot, value); }

	private PlotModel _thetaPlot;
	public PlotModel ThetaPlot { get => _thetaPlot; private set => SetProperty(ref _thetaPlot, value); }

	private PlotModel _alphaBal;
	public PlotModel AlphaBal { get => _alphaBal; private set => SetProperty(ref _alphaBal, value); }

	private PlotModel _deltaVBal;
	public PlotModel DeltaVBal { get => _deltaVBal; private set => SetProperty(ref _deltaVBal, value); }

	public ICommand RunCommand { get; }
	public ICommand RefreshAllCommand { get; }

	private readonly AircraftGeometryAndInertia _aircraft = new();
	private readonly FlightAndAeroParams _flight = new();
	private readonly DiffEqCoefficientsCalculator _coeff = new();
	private readonly CalculateControlLaw _controlLaw = new(new ControlLawParams());
	private readonly Simulation _sim;

	public MainWindowViewModel()
	{
		_sim = new Simulation(_coeff, _controlLaw);
		RunCommand = new RelayCommand(RunSimulationFromInputs);
		RefreshAllCommand = new RelayCommand(RefreshAll);
		RunSimulationFromInputs();
	}

	private void RunSimulationFromInputs()
	{
		var state = new AircraftState(_aircraft);

		var result = _sim.Run(
			aircraftParams: _aircraft,
			flightParams: _flight,
			state: state,
			tEnd: TEnd,
			dt: Dt,
			tStartDropping: TStartDropping,
			hz: HSet,
			controlLawNumber: LawNum,
			aCargo: ACargo,
			lCabin: LCabin
		);

		HeightPlot = BuildPlotModel(
			"Изменение высоты H(t)", "t, с", "0.0", "H, м", "0",
			"H(t)", result.Time.Zip(result.H, (t, y) => new DataPoint(t, y)).ToList());

		DvPlot = BuildPlotModel(
			"Изменения руля высоты ΔV(t)", "t, с", "0.0", "ΔV, °", "0.0",
			"ΔV(t)", result.Time.Zip(result.Dv, (t, y) => new DataPoint(t, y)).ToList());

		AlphaPlot = BuildPlotModel(
			"Угол атаки α(t)", "t, с", "0.0", "α, °", "0.0",
			"α(t)", result.Time.Zip(result.Alpha, (t, y) => new DataPoint(t, y)).ToList());

		ThetaPlot = BuildPlotModel(
			"Тангаж θ(t)", "t, с", "0.0", "θ, °", "0.0",
			"θ(t)", result.Time.Zip(result.Theta, (t, y) => new DataPoint(t, y)).ToList());

		AlphaBal = BuildPlotModel(
			"Балансировочный угол атаки αбал(t)", "t, с", "0.0", "α, °", "0.0",
			"αбал(t)", result.Time.Zip(result.AlphaBal, (t, y) => new DataPoint(t, y)).ToList());

		DeltaVBal = BuildPlotModel(
			"Балансировочный руль высоты ΔVбал(t)", "t, с", "0.0", "ΔV, °", "0.0",
			"ΔVбал(t)", result.Time.Zip(result.DeltaVBal, (t, y) => new DataPoint(t, y)).ToList());
	}

	private void RefreshAll()
	{
		Invalidate(HeightPlot);
		Invalidate(DvPlot);
		Invalidate(AlphaPlot);
		Invalidate(ThetaPlot);
		Invalidate(AlphaBal);
		Invalidate(DeltaVBal);
	}

	private static void Invalidate(PlotModel m) => m?.InvalidatePlot(true);

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? name = null)
	{
		if (Equals(storage, value)) return false;
		storage = value;
		OnPropertyChanged(name);
		return true;
	}

	private static PlotModel BuildPlotModel(
		string title,
		string xAxisTitle, string xAxisFormat,
		string yAxisTitle, string yAxisFormat,
		string seriesTitle,
		System.Collections.Generic.List<DataPoint> dataPoints)
	{
		var model = new PlotModel
		{
			Title = title,
			TextColor = OxyColors.White,
			PlotAreaBorderColor = OxyColor.FromRgb(36, 48, 63),
			Background = OxyColor.FromRgb(15, 20, 26),
			IsLegendVisible = false
		};

		model.Axes.Add(CreateAxis(AxisPosition.Bottom, xAxisTitle, xAxisFormat));
		model.Axes.Add(CreateAxis(AxisPosition.Left, yAxisTitle, yAxisFormat));

		var series = new LineSeries
		{
			Title = seriesTitle,
			Color = OxyColor.FromRgb(255, 165, 0),
			StrokeThickness = 2.2
		};
		series.Points.AddRange(dataPoints);
		model.Series.Add(series);

		return model;
	}

	private static LinearAxis CreateAxis(AxisPosition pos, string title, string format) =>
		new()
		{
			Position = pos,
			Title = title,
			MajorGridlineStyle = LineStyle.Solid,
			MinorGridlineStyle = LineStyle.Dot,
			MajorGridlineColor = OxyColor.FromRgb(40, 54, 70),
			MinorGridlineColor = OxyColor.FromRgb(28, 38, 50),
			StringFormat = format,
			TextColor = OxyColors.White,
			TitleColor = OxyColors.White
		};
}

