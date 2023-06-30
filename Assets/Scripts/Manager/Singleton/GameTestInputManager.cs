using Cinemachine;
using UnityEngine;


public class GameTestInputManager : MonoBehaviour
{
	private void Start()
	{
		GameManager.Sound.PlayBGM("Ambient");

		GameManager.UI.StackPanel<Panel_Network>();
	}


	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			FindObjectOfType<MonsterGenerator>().Generate();
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			FindObjectOfType<MonsterActor>().Move(this.transform, (MoveType)UnityEngine.Random.Range(0, 2));
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			FindObjectOfType<MonsterActor>().Stop();
		}

		if (Input.GetKeyDown(KeyCode.Z))
		{
			CinemachineSwitcher.SwitchMainCamera();
		}

		if (Input.GetKeyDown(KeyCode.X))
		{
			var virtualCamrea = GameObject.Find("1").GetComponent<CinemachineVirtualCamera>();

			CinemachineSwitcher.SwitchCamera(virtualCamrea);
		}
	}
}