using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerInputAction.IGameplayActions
{
	private PlayerInputAction _action;
	[SerializeField] private GameObject _selector;
	[SerializeField] private FleetSpawner _spawner;

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
		Vector2 mousePosition = context.ReadValue<Vector2>();
		Ray ray = Camera.main.ScreenPointToRay(mousePosition);
		if (GridMap.Instance.TryGetWorldPosCell(ray, out Vector3 gridPos))
		{
			_selector.SetActive(true);
			_selector.transform.position = gridPos;
		}
		else
			_selector.SetActive(false);
	}
}
