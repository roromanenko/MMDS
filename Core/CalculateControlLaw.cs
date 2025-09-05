using Core.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public class CalculateControlLaw
	{
		public ControlLawParams ControlLawParams { get; }

		public CalculateControlLaw(ControlLawParams controlLawParams)
		{
			ControlLawParams = controlLawParams;
		}

		public double CalculateFirstLaw(double h, double hz, double OmegaZ)
		{
			double dv;
			dv = (ControlLawParams.Kh * (h - hz))
				+ ControlLawParams.KOmegaZ * OmegaZ;

			return dv;
		}

		public double CalculateSecondLaw(double h, double hz, double OmegaZ, double hDot)
		{
			double dv = (ControlLawParams.Kh * (h - hz))
				+ ControlLawParams.KhDot * hDot
				+ ControlLawParams.KOmegaZ * OmegaZ;
			return dv;
		}

		public double CalculateThirdLaw(double h, double hz, double OmegaZ, double Theta)
		{
			double dv = (ControlLawParams.Kh * (h - hz))
				+ ControlLawParams.KTheta * (Theta - ControlLawParams.SmallThetaZero)
				+ ControlLawParams.KOmegaZ * OmegaZ;
			return dv;
		}

		public double CalculateFourthLaw(double h, double hz, double OmegaZ, double y)
		{
			double dv = (ControlLawParams.Kh * (h - hz))
				+ ControlLawParams.KTheta * y
				+ ControlLawParams.KOmegaZ * OmegaZ;
			return dv;
		}

		public double CalculateFifthLaw(double h, double hz, double OmegaZ, double hDot, double dt)
		{
			double dv = (ControlLawParams.Kh * (h - hz))
				+ ControlLawParams.KhDot * hDot
				+ ControlLawParams.KIntegral * ((h - hz) / dt)
				+ ControlLawParams.KOmegaZ * OmegaZ;
			return dv;
		}
	}
}
