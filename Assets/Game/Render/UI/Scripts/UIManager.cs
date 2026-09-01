using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject _rootObject;
	private List<IInteractibleUI> _interactibleUIs = new();
	public static UIManager Instance { get; private set; }
	public bool IsInteractingWithUI
	{
		get
		{
			if (_interactibleUIs == null || _interactibleUIs.Count == 0) return false;
			for (int i = 0; i < _interactibleUIs.Count; i++)
			{
				Debug.Log($"{_interactibleUIs[i].IsInteractingWithUI}");
				if (_interactibleUIs[i] != null && _interactibleUIs[i].IsInteractingWithUI)
					return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);

		if (_rootObject == null)
			_rootObject = gameObject;
		_interactibleUIs.AddRange(_rootObject.GetComponentsInChildren<IInteractibleUI>());
	}
}
