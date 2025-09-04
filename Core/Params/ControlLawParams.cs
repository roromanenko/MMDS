using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Params
{
	public sealed class ControlLawParams
	{
		public double Kh { get; init; } = 0.1;
		public double KhDot { get; init; } = 0.5;
		public double KOmegaZ { get; init; } = 1.0;
		public double KTheta { get; init; } = 1.0;
		public double KIntegral { get; init; } = 0.002;

		public double T1 { get; init; } = 20.0;
		public double T2 { get; init; } = 20.0;

		public double SmallThetaZero { get; init; } = 0;
	}
}
