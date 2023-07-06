using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class GameTestInputManager : MonoBehaviour
{
	private void Start()
	{
		GameManager.Sound.PlayBGM("Space", 1f);
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			FindObjectOfType<MonsterGenerator>().Generate(1);
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			FindObjectOfType<MonsterGenerator>().Generate(20f);
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			var monsters = FindObjectsOfType<MonsterActor>();

			for(int i = 0; i < monsters.Length;i ++)
			{
				monsters[i].Die();
			}
		}
	}
}