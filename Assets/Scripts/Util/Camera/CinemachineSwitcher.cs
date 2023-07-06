using Cinemachine;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineSwitcher
{
	private static CinemachineVirtualCamera current = null;
	private static CoroutineHandle handle_match;

	public static CinemachineVirtualCamera GetCurrentCamera()
    {
		return current;
	}
	
	public static void SwitchCamera(CinemachineVirtualCamera _camera, CinemachineBlendDefinition.Style _blendStyle = CinemachineBlendDefinition.Style.EaseInOut, float speed = 2f)
	{
		SetWatchSpeed(_blendStyle, speed);

		if (current == null)
		{
			current = _camera;
			current.Priority = 1;
		}

		else
		{
			_camera.Priority = 1;

			if(_camera != current)
			{
				current.Priority = 0;
				current = _camera;
			}
		}

		Timing.KillCoroutines(handle_match);

		handle_match = Timing.RunCoroutine(Co_MatchCamera(_camera), handle_match.GetHashCode());
	}

	public static void SwitchMainCamera(CinemachineBlendDefinition.Style _blendStyle = CinemachineBlendDefinition.Style.Cut, float _speed = 2f)
	{
		SetWatchSpeed(_blendStyle, _speed);

		if (current != null)
		{
			current.Priority = 0;
			current = null;
		}

		var virtualCam = PlayerController.Instance.virtualCamera;
		virtualCam.Priority = 1;

		Timing.KillCoroutines(handle_match);

		handle_match = Timing.RunCoroutine(Co_MatchCamera(virtualCam), handle_match.GetHashCode());
	}

	public static void SetWatchSpeed(CinemachineBlendDefinition.Style _blendStyle, float _speed)
	{
		PlayerController.Instance.cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(_blendStyle, _speed);
	}

	private static IEnumerator<float> Co_MatchCamera(CinemachineVirtualCamera _current)
	{
		yield return Timing.WaitUntilTrue(()=> 
			Vector3.Distance(PlayerController.Instance.mainCamera.transform.position, _current.transform.position) <= 0.001f &&
			Quaternion.Dot(PlayerController.Instance.mainCamera.transform.rotation, _current.transform.rotation) <= 0.001f
		);

		Debug.Log("Matched");
	}

	public static bool IsSwitched()
	{
		return handle_match.IsRunning;
	}
}
