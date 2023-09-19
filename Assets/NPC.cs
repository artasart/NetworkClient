using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
	[Range(0, 50)] public float walkSpeed = 4;
	[Range(0, 50)] public float runSpeed = 8;
	[Range(0, 1)] public float gravity = -12;
	[Range(0, 1)] public float jumpHeight = 1;
	[Range(0, 1)] public float airControlPercent;

	public float turnSmoothTime = 0.2f;
	private float turnSmoothVelocity;
	private readonly float speedSmoothTime = 0.15f;
	private float speedSmoothVelocity;
	private float currentSpeed;
	private readonly float standJumpDelay = .85f;
	private bool isJumping = false;
	private float velocityY;
	private float eulerY = 0;
	private bool isRotationFixed = false;
	private CharacterController controller;

	public Animator animator { get; set; }


	private void OnDestroy()
	{
		Timing.KillCoroutines(nameof(Co_Move));
	}

	// Start is called before the first frame update
	void Start()
	{
		controller = GetComponent<CharacterController>();
		animator = GetComponentInChildren<Animator>();

		Timing.RunCoroutine(Co_Move().Delay(UnityEngine.Random.Range(0.5f, 1.5f)), nameof(Co_Move));
	}


	IEnumerator<float> Co_Move()
	{
		var elapsedTime = 0f;
		var updateTime = 0f;
		var moveDir = Vector2.zero;

		while (true)
		{
			if (elapsedTime > updateTime)
			{
				moveDir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));

				updateTime = UnityEngine.Random.Range(0f, 3f);

				elapsedTime = 0f;
			}

			Move(moveDir, false);

			float movement = false ? currentSpeed / runSpeed : currentSpeed / walkSpeed * 0.5f;

			animator.SetFloat(Define.MOVEMENT, movement, speedSmoothTime, Time.deltaTime);

			elapsedTime += Time.deltaTime;

			yield return Timing.WaitForOneFrame;
		}
	}

	private void Move(Vector2 inputDir, bool running)
	{
		if (inputDir != Vector2.zero)
		{
			float targetRotation = (Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg);

			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
		}

		float targetSpeed = (running ? runSpeed : walkSpeed) * inputDir.magnitude;

		currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

		velocityY += Time.deltaTime * gravity;

		Vector3 velocity = (transform.forward * currentSpeed) + (Vector3.up * velocityY);

		DebugManager.ClearLog(velocity);

		controller.Move(velocity * Time.deltaTime);

		currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

		if (controller.isGrounded)
		{
			velocityY = 0;
		}
	}

	private float GetModifiedSmoothTime(float _smoothTime)
	{
		if (controller.isGrounded)
		{
			return _smoothTime;
		}

		return airControlPercent == 0 ? float.MaxValue : _smoothTime / airControlPercent;
	}
}
