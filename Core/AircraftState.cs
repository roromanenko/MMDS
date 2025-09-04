using Core.Params;

namespace Core
{
	public sealed class AircraftState
	{
		private readonly AircraftGeometryAndInertia _geom;

		public bool IsDropppedStart { get; private set; } = false;
		public bool IsDropped { get; private set; } = false;

		public AircraftState(AircraftGeometryAndInertia geom)
		{
			_geom = geom;
		}

		public void Reset()
		{
			IsDropped = false;
		}

		public void StartDropping()
		{
			IsDropppedStart = true;
		}

		public void TriggerInstantDrop()
		{
			IsDropped = true;
		}

		// Текущие значения (ступенька: до/после)
		public double CurrentMassKg =>
			IsDropped ? _geom.FlightMassAfterDropKg : _geom.FlightMassBeforeDropKg;

		public double CurrentIz =>
			IsDropped ? _geom.LongitudinalInertiaAfterDropKgM2 : _geom.LongitudinalInertiaBeforeDropKgM2;
	}
}
