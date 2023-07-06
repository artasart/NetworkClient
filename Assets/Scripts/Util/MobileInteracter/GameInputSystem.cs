using UnityEngine;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(InputSystemUIInputModule))]
public class GameInputSystem : MonoBehaviour
{
	#region Singleton

	public static GameInputSystem Instance
	{
		get
		{
			if (instance != null) return instance;
			instance = FindObjectOfType<GameInputSystem>();
			return instance;
		}
	}
	private static GameInputSystem instance;

	#endregion

	public static GameInputs inputs;
	private static bool isInitialized;

	public static bool Initialized { get { return isInitialized; } private set { isInitialized = value; } }

	private void Awake()
	{
		inputs = new GameInputs();

		isInitialized = true;
	}


	private void OnEnable()
	{
		if (inputs == null) return;

		inputs.Enable();
	}

	private void OnDisable()
	{
		if (inputs == null) return;

		inputs.Disable();
	}
}
