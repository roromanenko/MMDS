using Core.Coefficient;
using Core.Params;
using System.Globalization;
using System.Text;

namespace Core
{
	public sealed class SimulationResult
	{
		public List<double> Time { get; }
		public List<double> H { get; }
		public List<double> Dv { get; }
		public List<double> Alpha { get; }
		public List<double> Theta { get; }
		public List<double> AlphaBal { get; }
		public List<double> DeltaVBal { get; }
		public List<double> Xt { get; }
		public List<double> Ny { get; }

		public SimulationResult(List<double> time, List<double> h,
			List<double> dv, List<double> alpha, List<double> theta,
			List<double> alphaBal, List<double> deltaVBal, List<double> ny, List<double> xt)
		{
			Time = time;
			H = h;
			Dv = dv;
			Alpha = alpha;
			Theta = theta;
			AlphaBal = alphaBal;
			DeltaVBal = deltaVBal;
			Ny = ny;
			Xt = xt;
		}
	}

	public class Simulation
	{
		private readonly DiffEqCoefficientsCalculator _coeff;
		private readonly CalculateControlLaw _controlLaw;

		public Simulation(DiffEqCoefficientsCalculator coeff, CalculateControlLaw controlLaw)
		{
			_coeff = coeff;
			_controlLaw = controlLaw;
		}

		private (double AlphaBal, double DeltaVBal) CalculateBalanceValues(
			double mass,
			double wingArea,
			double airDensity,
			double velocity,
			double cy0,
			double cyAlpha,
			double mz0,
			double mzAlpha,
			double mzDeltaV,
			double xtCurrent,
			double xtNeutral = 0.24)
		{
			double cyBal = (2 * mass) / (wingArea * airDensity * Math.Pow(velocity, 2));
			double alphaBal = 57.3 * (cyBal - cy0) / cyAlpha;
			double deltaVBal = -57.3 * (mz0 + mzAlpha * alphaBal / 57.3 + cyBal * (xtCurrent - xtNeutral)) / mzDeltaV;

			return (alphaBal, deltaVBal);
		}

		public SimulationResult Run(AircraftGeometryAndInertia aircraftParams, FlightAndAeroParams flightParams, AircraftState state, double tEnd, double dt, double tStartDropping, double hz, int controlLawNumber, double aCargo, double lCabin)
		{
			var time = new List<double>();
			var smallTheta = new List<double>();
			var smallThetaDot = new List<double>();
			var smallThetaDotDot = new List<double>();
			var theta = new List<double>();
			var thetaDot = new List<double>();
			var alpha = new List<double>();
			var alphaDot = new List<double>();
			var deltaV = new List<double>();
			var deltaVDot = new List<double>();
			var h = new List<double>();
			var hDot = new List<double>();
			var ny = new List<double>();
			var alphaBal = new List<double>();
			var deltaVBal = new List<double>();
			var xtArr = new List<double>();

			double[] y = new double[16];
			double[] x = new double[16];

			y[9] = flightParams.Altitude0;

			// Расчет начальных балансировочных значений (они остаются постоянными как опорные)
			var (alphaBal1, deltaVBal1) = CalculateBalanceValues(
				aircraftParams.FlightMassBeforeDropKg,
				aircraftParams.WingAreaSqM,
				flightParams.AirDensityKgS2M4,
				flightParams.VelocityMS,
				flightParams.Cy0,
				flightParams.CyAlpha,
				flightParams.Mz0,
				flightParams.MzAlpha,
				flightParams.MzDeltaV,
				aircraftParams.CgBeforeDropPercentMac
			);

			double xt = aircraftParams.CgBeforeDropPercentMac;

			for (double t = 0; t < tEnd;)
			{
				double ku = (aircraftParams.CgAtReleasePercentMac - aircraftParams.CgBeforeDropPercentMac) / lCabin;
				double deltaXt = 0.0;

				double dv = controlLawNumber switch
				{
					1 => _controlLaw.CalculateFirstLaw(y[9], hz, y[2]),
					2 => _controlLaw.CalculateSecondLaw(y[9], hz, y[2], x[9]),
					3 => _controlLaw.CalculateThirdLaw(y[9], hz, y[2], y[1]),
					4 => _controlLaw.CalculateFourthLaw(y[9], hz, y[2], x[14]),
					5 => _controlLaw.CalculateFifthLaw(y[9], hz, y[2], x[9], y[15]),
					_ => _controlLaw.CalculateFirstLaw(y[9], hz, y[2])
				};

				if (t >= tStartDropping)
				{
					state.StartDropping();
				}

				if (state.IsDropppedStart && !state.IsDropped)
				{
					x[11] = y[12];
					x[12] = aCargo;
					deltaXt = ku * y[11];
					xt = aircraftParams.CgBeforeDropPercentMac + deltaXt;
				}
				else if (state.IsDropped)
				{
					xt = aircraftParams.CgAfterDropPercentMac;
					deltaXt = 0.0;
				}

				if (y[11] >= lCabin)
				{
					state.TriggerInstantDrop();
				}

				// Расчет текущих балансировочных значений на основе текущего состояния
				double currentMass = state.CurrentMassKg;
				double currentXt = state.IsDropped ? aircraftParams.CgAfterDropPercentMac : aircraftParams.CgBeforeDropPercentMac;

				var (alphaBal2, deltaVBal2) = CalculateBalanceValues(
					currentMass,
					aircraftParams.WingAreaSqM,
					flightParams.AirDensityKgS2M4,
					flightParams.VelocityMS,
					flightParams.Cy0,
					flightParams.CyAlpha,
					flightParams.Mz0,
					flightParams.MzAlpha,
					flightParams.MzDeltaV,
					currentXt
				);

				// Получаем коэффициенты для текущего состояния
				var coeffs = _coeff.ComputeCoefficients(flightParams, aircraftParams, state, x[5], dv, xt);
				var c = coeffs.C;

				// Расчет производных
				x[0] = y[1];
				x[1] = y[2];
				x[2] = -c[1] * x[1]
						- c[2] * x[6]
						- c[5] * x[5]
						- c[3] * x[7]
						+ c[20] * deltaXt;
				x[3] = y[4];
				x[4] = c[4] * x[6] + c[9] * x[7];
				x[5] = x[1] - x[4];
				x[6] = y[5] + (alphaBal1 - alphaBal2);  // Используем постоянный alphaBal1
				x[7] = dv + (deltaVBal1 - deltaVBal2);   // Используем постоянный deltaVBal1
				x[8] = y[9];
				x[9] = c[6] * y[4];
				x[10] = c[16] * x[4];

				x[13] = (y[1] - y[13]) / _controlLaw.ControlLawParams.T1;
				x[14] = y[1] - y[13];
				x[15] = y[9] - hz;

				// Интегрирование методом Эйлера
				for (int k = 0; k < x.Length; k++)
				{
					y[k] += x[k] * dt;
				}

				// Сохраняем данные
				smallTheta.Add(y[1]);
				smallThetaDot.Add(y[2]);
				smallThetaDotDot.Add(x[2]);
				theta.Add(y[4]);
				thetaDot.Add(x[4]);
				alpha.Add(y[5]);
				alphaDot.Add(x[6]);
				deltaV.Add(dv);
				deltaVDot.Add(x[7]);
				h.Add(y[9]);
				hDot.Add(x[9]);
				ny.Add(x[10]);
				alphaBal.Add(alphaBal2);
				deltaVBal.Add(deltaVBal2);
				xtArr.Add(xt);

				time.Add(t);
				t += dt;
			}

			return new SimulationResult(time, h, deltaV, alpha, theta, alphaBal, deltaVBal, ny, xtArr);
		}
	}
}