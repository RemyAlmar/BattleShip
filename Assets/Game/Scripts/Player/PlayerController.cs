using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerInputAction.IGameplayActions
{
	private PlayerInputAction _action;
	[SerializeField] private GameObject _selector;

	public Vector2 MousePosition { get; private set; }

	private void Awake()
	{
		_action ??= new();
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

		if (!context.canceled || UIManager.Instance.IsInteractingWithUI) return;

		Ray ray = Camera.main.ScreenPointToRay(MousePosition);

		if (GridMap.Instance.TryGetHoveredCell(ray, out Vector2Int gridPos))
		{
			Cell cell = GridMap.Instance.GetCell(gridPos.x, gridPos.y);
			EventBus.Invoke(new CellClickedEvent(cell));
		}
	}
}

public struct CellClickedEvent
{
	public Cell Cell { get; private set; }

	public CellClickedEvent(Cell cell)
	{
		this.Cell = cell;
	}
}