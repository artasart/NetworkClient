using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TouchInteracter : MonoBehaviour
{
	[SerializeField] protected QueryTriggerInteraction triggerInteraction;
	[SerializeField] protected LayerMask ignoreLayerMask;
	[SerializeField] private float maxInteractDistance = 100f;

	protected Camera overlayCamera;
	protected static Vector3 pointerPosition;
	protected readonly RaycastHit emptyHit = new RaycastHit();




	protected virtual void Awake()
	{

	}

	protected virtual void Start()
	{
		InitInputs();

		overlayCamera = Camera.main;
	}

	public void Refresh()
	{
		RemoveInputs();

		InitInputs();

		overlayCamera = Camera.main;
	}

	protected void InitInputs()
	{
		GameInputSystem.inputs.Touch.SingleTap.started += OnClickStarted;
		GameInputSystem.inputs.Touch.SingleTap.canceled += OnClickCancled;
		GameInputSystem.inputs.Touch.SingleTap.performed += OnClickPerformed;
	}

	private void RemoveInputs()
	{
		GameInputSystem.inputs.Touch.SingleTap.started -= OnClickStarted;
		GameInputSystem.inputs.Touch.SingleTap.canceled -= OnClickCancled;
		GameInputSystem.inputs.Touch.SingleTap.performed -= OnClickPerformed;
	}

	protected virtual void OnClickStarted(InputAction.CallbackContext callback)
	{
	}

	protected virtual void OnClickCancled(InputAction.CallbackContext callback)
	{
	}

	protected virtual void OnClickPerformed(InputAction.CallbackContext callback)
	{

	}

	protected virtual void Update()
	{
		if (!GameInputSystem.Initialized) return;

		pointerPosition = GameInputSystem.inputs.Touch.Point.ReadValue<Vector2>();
	}

	public RaycastHit GetWorldPositionHitData(Vector3 screenPosition)
	{
		if (!overlayCamera) return emptyHit;

		if (GameInputSystem.inputs.Touch.SecondaryTouch.triggered) return emptyHit;

		var ray = overlayCamera.ScreenPointToRay(screenPosition);

		Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance, ~ignoreLayerMask, triggerInteraction);

		return hit;
	}

	public static bool IsPointerOverUI()
	{
		var eventData = new PointerEventData(EventSystem.current)
		{
			position = (Vector2)pointerPosition
		};

		var raycastResult = new List<RaycastResult>();

		EventSystem.current.RaycastAll(eventData, raycastResult);

		return raycastResult.Any(result => result.gameObject.layer == LayerMask.NameToLayer("UI"));
	}
}

