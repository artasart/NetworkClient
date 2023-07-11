using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class GameTestInputManager : MonoBehaviour
{
	private void Start()
	{
		GameManager.UI.StackPanel<Panel_GameStart>();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Z))
		{
			GameClientManager.Instance.CreateRide();
		}

		if (Input.GetKeyDown(KeyCode.X))
		{
			GameClientManager.Instance.DestroyRide();
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			GameClientManager.Instance.CreateMain("ARTASART");
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			GameClientManager.Instance.DestroyMain();

		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			GameClientManager.Instance.CreateDummy();
		}

		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			GameClientManager.Instance.DestroyDummy();
		}
	}
}