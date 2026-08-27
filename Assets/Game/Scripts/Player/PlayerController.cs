using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerInputAction.IGameplayActions
{
	private PlayerInputAction _action;
	[SerializeField] private GameObject _selector;
	[SerializeField] private FleetSpawner _spawner;
	private IGridOccupant _shipSelected;
	[SerializeField, Range(-1, 5)] private int _shipTargetSpeed;
	[SerializeField, Range(-5, 5)] private int _shipDirection;
	[SerializeField] private bool _stopShip;

	public Vector2 MousePosition { get; private set; }

	private void Awake()
	{
		_action ??= new();
	}
	private void Start()
	{
		_spawner.SpawnFleet();
	}

	private void OnEnable()
	{
		_action?.Enable();
		_action?.Gameplay.SetCallbacks(this);
	}

	private void OnDisable()
	{
		_action?.Gameplay.RemoveCallbacks(this);
		_action?.Disable();
	}
	public void OnMousePosition(InputAction.CallbackContext context)
	{
		MousePosition = context.ReadValue<Vector2>();
		Ray ray = Camera.main.ScreenPointToRay(MousePosition);
		if (GridMap.Instance.TryGetWorldPosCell(ray, out Vector3 gridPos))
		{
			_selector.SetActive(true);
			_selector.transform.position = gridPos;
		}
		else
			_selector.SetActive(false);
	}

	public void OnInteract(InputAction.CallbackContext context)
	{

		if (!context.canceled) return;

		Ray ray = Camera.main.ScreenPointToRay(MousePosition);

		if (GridMap.Instance.TryGetHoveredCell(ray, out Vector2Int gridPos))
		{
			Cell cell = GridMap.Instance.GetCell(gridPos.x, gridPos.y);
			if (cell.IsOccupied)
			{
				_shipSelected = cell.Occupant != _shipSelected ? cell.Occupant : null;
			}
			else if (!cell.IsOccupied && _shipSelected != null)
			{
				if (_shipSelected is IGridMovable movableShip)
				{
					MoveOrder order = new(_shipTargetSpeed, _shipDirection, _stopShip);
					movableShip.ExecuteOrder(order);
				}
			}

		}
	}
}
