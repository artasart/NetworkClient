using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum InteractionType
{
	CameraDistance,
}

[RequireComponent(typeof(Collider))]
public class TouchInteractable : MonoBehaviour
{
	#region Members

	public InteractionType interactionType = InteractionType.CameraDistance;
	public float distance = 8f;
	public float touchDelay = 1;

	private UnityEvent touchEvent = new UnityEvent();

	CoroutineHandle handle_interact;

	#endregion



	#region Core Methods

	public bool IsInteractable(float _distance = 0)
	{
		switch (interactionType)
		{
			case InteractionType.CameraDistance:
				if (_distance <= distance) return true;
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}

		return false;
	}

	public void Interact()
	{
		if (handle_interact.IsRunning) return;

		handle_interact = Timing.RunCoroutine(Co_Interact());
	}

	IEnumerator<float> Co_Interact()
	{
		touchEvent.Invoke();

		yield return Timing.WaitForSeconds(touchDelay);
	}

	#endregion



	#region Basic Methods

	public void AddEvent(UnityAction _action)
	{
		touchEvent.AddListener(_action);
	}

	public void RemoveEvent(UnityAction _action)
	{
		touchEvent.RemoveListener(_action);
	}

	public void RemoveAllEvent()
	{
		touchEvent.RemoveAllListeners();
	}

	#endregion
}
