using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Params
{
	public sealed class FlightAndAeroParams
	{
		// Flight conditions
		public double VelocityMS { get; init; } = 97.2;					// V0, m/s
		public double Altitude0 { get; init; } = 600;					// H0, m
		public double AirDensityKgS2M4 { get; init; } = 0.119;			// ρ, кг·с^2/м^4
		public double GravityMS2 { get; init; } = 9.81;					// g, m/s^2
		public double SpeedOfSoundMS { get; init; } = 338.36;			// aH, m/s

		// Aerodynamic coefficients
		public double Cy0 { get; init; } = -0.255;
		public double CyAlpha { get; init; } = 5.78;
		public double CyDeltaV { get; init; } = 0.2865;
		public double Cx { get; init; } = 0.13;

		// Pitching moment derivatives
		public double Mz0 { get; init; } = 0.2;
		public double MzOmegaZ { get; init; } = -13;
		public double MzAlphaDot { get; init; } = -3.8;
		public double MzAlpha { get; init; } = -1.83;
		public double MzDeltaV { get; init; } = -0.96;
	}
}
