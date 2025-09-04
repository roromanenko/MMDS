using Core.Coefficient;
using Core.Params;

namespace Core
{
	public sealed class SimulationResult
	{
		public List<double> Time { get; }
		public List<double> H { get; }

		public SimulationResult(List<double> time, List<double> h)
		{
			Time = time;
			H = h;
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

			// Инициализируем массивы с производными
			double[] y = new double[15];
			double[] x = new double[15];

			y[8] = flightParams.AltitudeM;

			double alphaBal1 = 0;
			double deltaVBal1 = 0;

			double xt = aircraftParams.CgBeforeDropPercentMac;

			// Основной цикл
			for (double i = 0; i < tEnd;)
			{
				double ku = (aircraftParams.CgAtReleasePercentMac - aircraftParams.CgBeforeDropPercentMac) / lCabin;
				double deltaXt = 0.0;

				double dv;
				// Считаем DeltaV исходя из закона управления
				if (controlLawNumber == 1)
				{
					dv = _controlLaw.CalculateFirstLaw(y[9], hz, y[2]);
				}
				else if (controlLawNumber == 2)
				{
					dv = _controlLaw.CalculateSecondLaw(y[9], hz, x[1]);
				}
				else if (controlLawNumber == 3)
				{
					dv = _controlLaw.CalculateThirdLaw(y[9], hz, x[1]);
				}
				else if (controlLawNumber == 4)
				{
					dv = _controlLaw.CalculateFourthLaw(y[9], hz, x[1]);
				}
				else if (controlLawNumber == 5)
				{
					dv = _controlLaw.CalculateFifthLaw(y[9], hz, x[1]);
				}
				else
				{
					dv = _controlLaw.CalculateFirstLaw(y[9], hz, x[1]);
				}

				// Инициация начала сброса
				if (i >= tStartDropping)
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

				if (i is 0)
				{
					alphaBal1 = alphaBal2;
					deltaVBal1 = deltaVBal2;
				}

				var c = coeffs.C;

				// Производные
				x[0] = y[1];                                                                        // ϑ
				x[1] = y[2];                                                                        // ϑ*
				x[2] = -c[1] * y[2] - c[2] * x[6]
						- c[5] * (y[2] - x[4])                   // α̇ = ϑ̇ - θ̇
						- c[3] * x[7]
						+ c[20] * deltaXt;                       // Δx̄T = k_u Sван
				x[3] = y[4];                                                                        // θ
				x[4] = c[4] * x[6] + c[9] * x[7];                                                   // θ*
				x[5] = y[1] - y[4];                                                                 // α
				x[6] = x[5] + (alphaBal1 - alphaBal2);                                              // α*
				x[7] = dv + (deltaVBal1 - deltaVBal2);                                              // δB*
				x[8] = y[9];                                                                        // H
				x[9] = c[6] * y[4];                                                                 // H*
				x[10] = c[16] * x[4];                                                               // ny

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
				alpha.Add(x[5]);
				alphaDot.Add(x[6]);
				deltaV.Add(dv);
				deltaVDot.Add(x[7]);
				h.Add(y[9]);
				hDot.Add(x[9]);
				ny.Add(x[10]);

				time.Add(i);
				i += dt;
			}

			return new SimulationResult(time, h);
		}
	}
}
