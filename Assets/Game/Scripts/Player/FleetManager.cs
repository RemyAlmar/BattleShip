using System;
using System.Collections.Generic;
using UnityEngine;

public class FleetManager : MonoBehaviour
{
	private IGridOccupant _shipSelected;
	private List<IGridOccupant> _fleet;
	[SerializeField, Range(-1, 5)] private int _shipTargetSpeed;
	[SerializeField, Range(-5, 5)] private int _shipDirection;
	[SerializeField] private bool _stopShip;

	private void Start() => EventBus.Invoke(GameEvent.StartGame);
	private void OnEnable()
	{
		EventBus.Subscribe<CellClickedEvent>(Execute);
		EventBus.Subscribe<FleetSpawnedEvent>(RegisterFleet);
	}
	private void OnDisable()
	{
		EventBus.Unsubscribe<CellClickedEvent>(Execute);
		EventBus.Unsubscribe<FleetSpawnedEvent>(RegisterFleet);
	}

	private void RegisterFleet(FleetSpawnedEvent data) => _fleet = new(data.Fleet);

	private void Execute(CellClickedEvent data)
	{
		SelectionCell(data.Cell);
		ExecuteOrder(data.Cell);
	}

	private void SelectionCell(Cell cell)
	{
		IGridOccupant lastSelected = _shipSelected;
		_shipSelected = cell.Occupant;
		ShipController shipController = _shipSelected as ShipController;

		SelectionState state = _shipSelected != lastSelected ? shipController != null ? SelectionState.Selected : SelectionState.Deselected : SelectionState.None;
		EventBus.Invoke(new ShipSelectedEvent(shipController, state));
	}
	private void ExecuteOrder(Cell cell)
	{
		if (!cell.IsOccupied && _shipSelected != null)
		{
			if (_shipSelected is IGridMovable movableShip)
			{
				MoveOrder order = new(_shipTargetSpeed, _shipDirection, _stopShip);
				movableShip.ExecuteOrder(order);
			}
		}
	}
}