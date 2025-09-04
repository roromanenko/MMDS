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
		private readonly ControlLawParams _controlLawParams;

		public CalculateControlLaw(ControlLawParams controlLawParams)
		{
			_controlLawParams = controlLawParams;
		}

		public double CalculateFirstLaw(double h, double hz, double OmegaZ)
		{
			double dv;
			dv = (_controlLawParams.Kh * (h - hz)) + _controlLawParams.KOmegaZ * OmegaZ;

			return dv;
		}

		public double CalculateSecondLaw(double h, double hz, double OmegaZ)
		{
			return 0;
		}

		public double CalculateThirdLaw(double h, double hz, double OmegaZ)
		{
			return 0;
		}

		public double CalculateFourthLaw(double h, double hz, double OmegaZ)
		{
			return 0;
		}

		public double CalculateFifthLaw(double h, double hz, double OmegaZ)
		{
			return 0;
		}
	}
}
