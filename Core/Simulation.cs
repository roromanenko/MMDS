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

		public SimulationResult(List<double> time, List<double> h,
			List<double> dv, List<double> alpha, List<double> theta,
			List<double> alphaBal, List<double> deltaVBal)
		{
			Time = time;
			H = h;
			Dv = dv;
			Alpha = alpha;
			Theta = theta;
			AlphaBal = alphaBal;
			DeltaVBal = deltaVBal;
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

		public SimulationResult Run(AircraftGeometryAndInertia aircraftParams, FlightAndAeroParams flightParams, AircraftState state, double tEnd, double dt, double tStartDropping, double hz, int controlLawNumber, double aCargo, double lCabin)
		{
			// Инициализируем списки с финальными данными
			var time = new List<double>();
			var smallTheta = new List<double>();                // ϑ
			var smallThetaDot = new List<double>();             // ϑ*
			var smallThetaDotDot = new List<double>();          // ϑ**
			var theta = new List<double>();                     // θ
			var thetaDot = new List<double>();                  // θ*
			var alpha = new List<double>();                     // α
			var alphaDot = new List<double>();                  // α*
			var deltaV = new List<double>();                    // δB
			var deltaVDot = new List<double>();                 // δB*
			var h = new List<double>();                         // H
			var hDot = new List<double>();                      // H*
			var ny = new List<double>();                        // ny
			var alphaBal = new List<double>();
			var deltaVBal = new List<double>();

			// Инициализируем массивы с производными
			double[] y = new double[15];
			double[] x = new double[15];

			y[9] = flightParams.Altitude0;

			double alphaBal1 = 0;
			double deltaVBal1 = 0;

			double xt = aircraftParams.CgBeforeDropPercentMac;

			StringBuilder logSb = new();
			logSb.AppendLine($"t|height|dv|alpha|theta|dvBalance|Ny|alphaBalance|xt|sCargo");
			// Основной цикл
			for (double t = 0; t < tEnd;)
			{
				double ku = (aircraftParams.CgAtReleasePercentMac - aircraftParams.CgBeforeDropPercentMac) / lCabin;
				double deltaXt = 0.0;

				// Считаем DeltaV исходя из закона управления
				double dv = controlLawNumber switch
				{
					1 => _controlLaw.CalculateFirstLaw(y[9], hz, y[2]),

					2 => _controlLaw.CalculateSecondLaw(y[9], hz, y[2], x[9]),
					3 => _controlLaw.CalculateThirdLaw(y[9], hz, y[2], y[1]),
					4 => _controlLaw.CalculateFourthLaw(y[9], hz, y[2], x[14]),
					5 => _controlLaw.CalculateFifthLaw(y[9], hz, y[2], x[9], dt),
					_ => _controlLaw.CalculateFirstLaw(y[9], hz, y[2])
				};

				// Инициация начала сброса
				if (t >= tStartDropping)
				{
					state.StartDropping();
				}

				// Обработка состояний центровки в процессе сброса
				if (state.IsDropppedStart && !state.IsDropped)
				{
					x[11] = y[12];											// Ṡвант
					x[12] = aCargo;											// S̈вант = aвант
					deltaXt = ku * y[11];									// Δx̄T = k_u * Sван
					xt = aircraftParams.CgBeforeDropPercentMac + deltaXt;
				}
				else if (state.IsDropped)
				{
					xt = aircraftParams.CgAfterDropPercentMac;
					deltaXt = 0.0;											// после схода груза Δx̄T = 0
				}

				if (y[11] >= lCabin)
				{
					state.TriggerInstantDrop();
				}

				// Инициализация расчёта коеф. и баланс. значений.
				var coeffs = _coeff.ComputeCoefficients(flightParams, aircraftParams, state, x[5], dv, xt);

				// Считаем AlphaBal и DeltaVBal
				double alphaBal2 = coeffs.AlphaBalance;
				double deltaVBal2 = coeffs.DeltaVBalance;

				if (t <= 0)
				{
					alphaBal1 = alphaBal2;
					deltaVBal1 = deltaVBal2;
				}

				var c = coeffs.C;

				// Производные
				x[0] = y[1];                                                                        // ϑ
				x[1] = y[2];                                                                        // ϑ'
				x[2] = -c[1] * x[1]
						- c[2] * x[6]                                                   // ϑ''
						- c[5] * x[5]                   // α̇ = ϑ̇ - θ̇
						- c[3] * x[7]
						+ c[20] * deltaXt;                       // Δx̄T = k_u Sван
				x[3] = y[4];                                                                        // θ
				x[4] = c[4] * x[6] + c[9] * x[7];                                                   // θ'
				x[5] = x[1] - x[4];                                                                 // α'
				x[6] = y[5] + (alphaBal1 - alphaBal2);                                              // α*
				x[7] = dv + (deltaVBal1 - deltaVBal2);                                              // δB*
				x[8] = y[9];                                                                        // H
				x[9] = c[6] * y[4];                                                                 // H'
				x[10] = c[16] * x[4];                                                               // ny

				x[13] = (y[1] - y[13]) / _controlLaw.ControlLawParams.T1; //x'
				x[14] = y[1] - y[13]; // y, law4

				// Шаг Ейлера
				for (int k = 0; k < x.Length; k++)
				{
					y[k] += x[k] * dt;
				}

				alphaBal1 = alphaBal2;
				deltaVBal1 = deltaVBal2;

				// Сохраняем данные в массив
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

				time.Add(t);
				t += dt;

				//Logs section
				logSb.AppendLine($"{FormatNumber(t)}|" +
					$"{FormatNumber(y[9])}|" +
					$"{FormatNumber(dv)}|" +
					$"{FormatNumber(x[5])}|" +
					$"{FormatNumber(y[1])}|" +
					$"{FormatNumber(deltaVBal1)}|" +
					$"{FormatNumber(x[10])}|" +
					$"{FormatNumber(alphaBal1)}|" +
					$"{FormatNumber(xt)}|" +
					$"{FormatNumber(y[11])}");
			}

			// File.WriteAllText(@"D:\Projects\RomaSim\LogsCheck\Logs1.csv", logSb.ToString());

			return new SimulationResult(time, h, deltaV, alpha, theta, alphaBal, deltaVBal);
		}

		private static string FormatNumber(double number)
		{
			return Math.Round(number, 2).ToString("F2", CultureInfo.InvariantCulture);
		}
	}
}
