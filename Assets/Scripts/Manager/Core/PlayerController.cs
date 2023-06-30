using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using MEC;

public class PlayerController : MonoBehaviour
{
	#region Singleton

	public static PlayerController Instance
	{
		get
		{
			if (instance != null) return instance;
			instance = FindObjectOfType<PlayerController>();
			return instance;
		}
	}
	private static PlayerController instance;

	#endregion



	#region Members

	[HideInInspector] public Camera mainCamera;
	[HideInInspector] public CinemachineVirtualCamera virtualCamera;
	[HideInInspector] public CinemachineBrain cinemachineBrain;
	[HideInInspector] public CameraShake cameraShake;

	Transform cameraParent;

	GameObject crystal;
	public float rotateSpeed = 45f;

	CoroutineHandle handle_rotate;

	#endregion



	#region Initialize

	private void OnDestroy()
	{
		Timing.KillCoroutines(handle_rotate);
	}

	private void Awake()
	{
		cameraParent = GameObject.Find("Camera").transform;

		mainCamera = Camera.main;
		cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
		virtualCamera = cameraParent.GetComponentInChildren<CinemachineVirtualCamera>();
		cameraShake = cameraParent.GetComponent<CameraShake>();

		crystal = GameObject.Find("HolyCrystal");
	}

	private void Start()
	{
		handle_rotate = Timing.RunCoroutine(Co_RotateCrystal());
	}

	#endregion



	#region Basic Methods

	private IEnumerator<float> Co_RotateCrystal()
	{
		while (true)
		{
			crystal.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}
	}

	#endregion
}
