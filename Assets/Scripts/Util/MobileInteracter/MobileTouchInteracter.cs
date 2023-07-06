using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MobileTouchInteracter : TouchInteracter
{
	protected override void Awake()
	{
		base.Awake();
		triggerInteraction = QueryTriggerInteraction.Collide;
	}

	protected override void OnClickPerformed(InputAction.CallbackContext callback)
	{
		if (!IsPointerOverUI()) Interact();
	}

	private void Interact()
	{
		var hit = GetWorldPositionHitData((Vector2)pointerPosition);

		if (hit.collider == null) return;

		if (hit.collider.TryGetComponent(out TouchInteractable interactable))
		{
			if (interactable.IsInteractable(hit.distance))
			{
				interactable.Interact();
			}
		}
	}
}

