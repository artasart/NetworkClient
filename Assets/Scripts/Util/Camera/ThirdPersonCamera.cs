using Cinemachine;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	public bool lockCursor = true;
	[Range(0f, 10f)] public float mouseSensitivity = 5;
	[Range(-10f, 10f)] public float cameraDistance = 5;
	[Range(1f, 10f)] public float zoomSpeed = 5f;
	[Range(0f, 1f)] public float zoomSmoothness = 0.1f;

	public Vector2 pitchMinMax = new Vector2(-20, 85);
	public Vector3 offset = new Vector3(0f, 1.75f, 0f);
	public Vector3 ZoomMaxOffset = new Vector3(0f, 1.75f, 0f);

	Transform target;

	Vector3 rotationSmoothVelocity;
	Vector3 currentRotation;

	float yaw;
	float pitch;

	float rotationSmoothTime = 0.12f;

	public float scrollAmount;
	CoroutineHandle handle_CameraDistance;

	public Transform test;

	private void Awake()
	{
		test = this.transform.parent.Search("LookTarget");
	}

	void Start()
	{
		target = FindObjectOfType<PlayerController>().transform;

		if (lockCursor)
		{
			ShowCursor(false);
		}
	}

	void LateUpdate()
	{
		if (!handle_CameraDistance.IsRunning) scrollAmount = Input.GetAxis("Mouse ScrollWheel");

		float targetDistance = cameraDistance - scrollAmount * zoomSpeed;

		cameraDistance = Mathf.Clamp(targetDistance, 0.5f, 10f);

		yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
		pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
		pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

		currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
		test.transform.eulerAngles = currentRotation;

		float currentDistance = Vector3.Distance(transform.position, target.position + offset);
		float smoothedDistance = Mathf.Lerp(currentDistance, cameraDistance, zoomSmoothness);
		transform.position = target.position + offset - transform.forward * smoothedDistance;
	}

	public void ShowCursor(bool isShow)
	{
		Cursor.lockState = isShow ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = isShow;
	}

	public void SetCameraDistance(float _target = 5f)
	{
		Timing.KillCoroutines(handle_CameraDistance);

		handle_CameraDistance = Timing.RunCoroutine(Co_SetCameraDistance(_target));
	}

	IEnumerator<float> Co_SetCameraDistance(float _target = 5f)
	{
		scrollAmount = .1f;

		float lerpvalue = 0f;

		while (Mathf.Abs(cameraDistance - _target) > 0.001f)
		{
			cameraDistance = Mathf.Lerp(cameraDistance, _target, lerpvalue += Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		cameraDistance = _target;
		scrollAmount = 0f;
	}
}