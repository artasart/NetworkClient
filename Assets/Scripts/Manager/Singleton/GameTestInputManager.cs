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
}