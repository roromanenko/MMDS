using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Params
{
	public sealed class AircraftGeometryAndInertia
	{
		//Geometry
		public double WingAreaSqM { get; init; } = 201.45;								//S, m^2
		public double MeanAerodynamicChordM { get; init; } = 5.285;						// MAC, m

		// Mass (kg)
		public double FlightMassBeforeDropKg { get; init; } = 73000;					//G1
		public double FlightMassAfterDropKg { get; init; } = 68000;						//G2

		// Center of Gravity (% of MAC)
		public double CgBeforeDropPercentMac { get; init; } = 0.24;						// x̄_t1
		public double CgAtReleasePercentMac { get; init; } = 0.30;						// x̄_c
		public double CgAfterDropPercentMac { get; init; } = 0.24;						// x̄_t2

		// Longitudinal moment of inertia (kg·m^2).
		public double LongitudinalInertiaBeforeDropKgM2 { get; init; } = 660000;		// I_z1
		public double LongitudinalInertiaAfterDropKgM2 { get; init; } = 650000;			// I_z2
	}
}
