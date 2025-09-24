using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Core;
using Core.Params;
using Core.Coefficient;


namespace Core
{
	internal class Program
	{
		static void Main(string[] args)
		{
			// 1) Исходные данные
			var aircraft = new AircraftGeometryAndInertia{ };

			var flight = new FlightAndAeroParams{ };

			// 2) Состояние самолёта
			var state = new AircraftState(aircraft);

			// 3) Коеф
			var coeff = new DiffEqCoefficientsCalculator();

			// 4) 
			var controlLawParams = new ControlLawParams{ };

			// 4) CalculateControlLaw
			var controlLaw = new CalculateControlLaw(controlLawParams);

			var sim = new Simulation(coeff, controlLaw);

			// 4) Параметры запуска моделирования
			double tEnd = 90;   // сек
			double dt = 0.01;   // сек
			double tStartDropping = 2;    // начало вытягивания груза (сек)
			double Hset = 600;    // Hзад (для законов управления)
			int lawNum = 1;      // номер закона управления (1..5)
			double aCargo = 0.3;    // a_ван (м/с^2)
			double lCabin = 10;    // L_каб (м)

			// 5) Вызов симуляции
			SimulationResult result = sim.Run(
				aircraftParams: aircraft,
				flightParams: flight,
				state: state,
				tEnd: tEnd,
				dt: dt,
				tStartDropping: tStartDropping,
				hz: Hset,
				controlLawNumber: lawNum,
				aCargo: aCargo,
				lCabin: lCabin
			);
			Console.WriteLine("Sim is finish, enter any button");
			Console.ReadKey();
		}
	}
}
