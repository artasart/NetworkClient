using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
	TouchInteractable touch;

	private void Awake()
	{
		touch = GetComponent<TouchInteractable>();

		touch.AddEvent(() => GameManager.Sound.PlaySound("Click_2"));
		touch.AddEvent(() => GameManager.UI.StackPanel<Panel_Casino>());
	}
}
