using Cinemachine;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineTPSController : MonoBehaviour
{
    public bool lockCursor = true;
    [Range(0f, 10f)] public float mouseSensitivity = 5f;
    [Range(0f, 100f)] public float zoomSpeed = 25f;
    [Range(0f, 1f)] public float zoomSmoothness = 0.1f;

    public Vector2 pitchMinMax = new Vector2(-20, 85);

    Transform target;
    Transform lookTarget;

    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    float rotationSmoothTime = 0.12f;

    float scrollAmount;

    CinemachineVirtualCamera virtualCamera;
    Cinemachine3rdPersonFollow thirdPersonFollow;

    CoroutineHandle handle_CameraDistance;

    private void Awake()
    {
        lookTarget = this.transform.parent.Search("LookTarget");

        virtualCamera = this.transform.parent.GetComponentInChildren<CinemachineVirtualCamera>();
        thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    void Start()
    {
        target = FindObjectOfType<PlayerController>().transform;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        lookTarget.transform.eulerAngles = currentRotation;

        var scrollValue = Input.GetAxis("Mouse ScrollWheel");

        if (!Equals(scrollAmount, scrollValue))
        {
            var distance = thirdPersonFollow.CameraDistance + scrollAmount * 50;

            distance = Mathf.Clamp(distance, 2f, 15f);

            SetCameraDistance(distance);
        }

        scrollAmount = scrollValue;

        transform.position = target.position + transform.forward;
	}

    public void ShowCursor(bool isShow)
    {
        Cursor.lockState = isShow ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = isShow;
    }

    public void SetCameraDistance(float _target = 5f)
	{
		Timing.KillCoroutines(handle_CameraDistance);

		handle_CameraDistance = Timing.RunCoroutine(Co_SetCameraDistance(_target));
	}

	IEnumerator<float> Co_SetCameraDistance(float _target = 5f)
	{
        float lerpvalue = 0f;

		while (Mathf.Abs(thirdPersonFollow.CameraDistance - _target) > 0.001f)
		{
			thirdPersonFollow.CameraDistance = Mathf.Lerp(thirdPersonFollow.CameraDistance, _target, lerpvalue += Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		thirdPersonFollow.CameraDistance = _target;
    }
}