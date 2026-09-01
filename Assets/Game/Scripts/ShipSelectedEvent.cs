public struct ShipSelectedEvent
{
	public ShipController Ship { get; private set; }
	public SelectionState State { get; private set; }
	public ShipSelectedEvent(ShipController ship, SelectionState state)
	{
		Ship = ship;
		State = state;
	}
}