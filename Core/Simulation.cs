using Core.Coefficient;
using Core.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core
{
	public class Simulation
	{
		private readonly DiffEqCoefficientsCalculator _coeff;

		public Simulation(DiffEqCoefficientsCalculator coeff)
		{
			_coeff = coeff;
		}

		public void Run(AircraftGeometryAndInertia aircraftParams, FlightAndAeroParams flightParams, AircraftState state, double tEnd, double dt)
		{
			// Инициализируем списки с финальными данными
			var time = new List<double>();
			var smallTheta = new List<double>();				// ϑ
			var smallThetaDot = new List<double>();				// ϑ*
			var smallThetaDotDot = new List<double>();			// ϑ**
			var theta = new List<double>();						// θ
			var thetaDot = new List<double>();					// θ*
			var alpha = new List<double>();						// α
			var alphaDot = new List<double>();					// α*
			var deltaV = new List<double>();					// δB
			var deltaVDot = new List<double>();					// δB*
			var h = new List<double>();							// H
			var hDot = new List<double>();						// H*
			var ny = new List<double>();						// ny

			// Инициализируем массивы с производными
			double[] y = new double[15];
			double[] x = new double[15];


			// Основной цикл
			for (double i = 0; i < tEnd;)
			{
				var c = _coeff.ComputeCoefficients(flightParams, aircraftParams, state, x[6], dV, xt);

				// Производные
				x[0] = y[1];																		// ϑ
				x[1] = y[2];																		// ϑ*
				x[2] = -c[1] * x[1] - c[2] * x[6] - c[5] * x[5] - c[3] * x[7] + c[20] * xt;			// ϑ**
				x[3] = y[4];																		// θ
				x[4] = c[4] * x[6] + c[9] * x[7];													// θ*
				x[5] = y[1] - y[4];																	// α
				x[6] = x[5] + AlphaBal;																// α*
				x[7] = dV + deltaVBal;																// δB*
				x[8] = y[9];																		// H
				x[9] = c[6] * y[4];																	// H*
				x[10] = c[16] * x[4];																// ny

				// Шаг Ейлера
				for (int k = 0; k < x.Length; k++)
				{
					y[k] += x[k] * dt;
				}

				// Сохраняем данные в массив
				smallTheta.Add(x[0]);
				smallThetaDot.Add(x[1]);
				smallThetaDotDot.Add(x[2]);
				theta.Add(x[3]);
				thetaDot.Add(x[4]);
				alpha.Add(x[5]);
				alphaDot.Add(x[6]);
				deltaV.Add(x[dV]);
				deltaVDot.Add(x[7]);
				h.Add(x[8]);
				hDot.Add(x[9]);
				ny.Add(x[10]);

				time.Add(i);
				i += dt;
			}
		}
	}
}
