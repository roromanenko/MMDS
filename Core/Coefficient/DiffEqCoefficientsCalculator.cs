using Core.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Coefficient
{
	public class DiffEqCoefficientsCalculator
	{
		public double[] ComputeCoefficients(FlightAndAeroParams flightParams, AircraftGeometryAndInertia aircraftParams, AircraftState state, double alpha, double deltaV)
		{
			var c = new double[21];

			double m = state.CurrentMassKg;
			double Iz = state.CurrentIz;
			double xt = state.CurrentCgPercentMac;

			double S = aircraftParams.WingAreaSqM;
			double b = aircraftParams.MeanAerodynamicChordM;

			double rho = flightParams.AirDensityKgS2M4;
			double V = flightParams.VelocityMS;
			double g = flightParams.GravityMS2;

			// Считаем балансированные значения
			double CyBalance = (2 * m) / (S * rho * Math.Pow(V, 2));
			double AlphaBalance = 57.3 * ((CyBalance - flightParams.Cy0) / flightParams.CyAlpha);
			double DeltaVBalance = -57.3 * (((flightParams.Mz0 + flightParams.MzAlpha * AlphaBalance) / (57.3 + CyBalance * (xt - 0.24))) / flightParams.MzDeltaV);


			double Cy = CyBalance + flightParams.CyAlpha * (alpha / 57.3) + flightParams.CyDeltaV * (deltaV / 57.3);

			c[1] = -(flightParams.MzOmegaZ / Iz) * S * Math.Pow(b, 2) * ((rho * V) / 2);
			c[2] = -(flightParams.MzAlpha / Iz) * S * b * ((rho * Math.Pow(V, 2)) / 2);
			c[3] = -(flightParams.MzDeltaV / Iz) * S * b * ((rho * Math.Pow(V, 2)) / 2);
			c[4] = ((flightParams.CyAlpha + flightParams.Cx) / m) * S * ((rho * V) / 2);
			c[5] = -(flightParams.MzAlphaDot / Iz) * S * Math.Pow(b, 2) * ((rho * V) / 2);
			c[6] = V / 57.3;
			c[9] = (flightParams.CyDeltaV / m) * S * ((rho * V) / 2);
			c[16] = V / (57.3 * g);
			c[20] = 57.3 * Cy * S * b * ((rho * Math.Pow(V, 2)) / 2 * Iz);

			return c;
		}
	}
}
