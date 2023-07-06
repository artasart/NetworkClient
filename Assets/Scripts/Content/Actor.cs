using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
	float moveSpeed = 1f;
	float walkSpeed = 1.5f;
	float runSpeed = 3f;
	float acceleration = 1.5f;

	float animationSpeed = 0f;
	float turnThreshold = 0.1f;

	bool isLookTarget = false;

	protected Animator animator;
	GameObject model;

	CoroutineHandle handle_move;
	CoroutineHandle handle_playMotion;

	private void Awake()
	{
		model = this.transform.Search(Define.MODEL).gameObject;

		animator = model.GetComponent<Animator>();
		animator.SetFloat(Define.MOVEMENT, 0f);
	}

	public void Move(Transform _destination, MoveType _moveType = MoveType.Walk)
	{
		isLookTarget = false;

		moveSpeed = _moveType == MoveType.Walk ? walkSpeed : runSpeed;

		Timing.KillCoroutines(handle_move);

		handle_move = Timing.RunCoroutine(Co_Move(_destination));
	}

	IEnumerator<float> Co_Move(Transform _target)
	{
		var animationMaxSpeed = (moveSpeed == walkSpeed) ? .5f : 1f;
		var moveSpeedMultiplier = 0f;

		while (Vector3.Distance(this.transform.position, _target.position) >= 0.001f)
		{
			var targetRotation = Quaternion.LookRotation(_target.position - transform.position);

			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, moveSpeed * Time.deltaTime);

			if (!isLookTarget && Quaternion.Angle(transform.rotation, targetRotation) < turnThreshold)
			{
				isLookTarget = true;

				this.transform.LookAt(_target);
			}

			animationSpeed = Mathf.MoveTowards(animationSpeed, animationMaxSpeed, acceleration * Time.deltaTime);

			animator.SetFloat(Define.MOVEMENT, animationSpeed);

			if (animationSpeed >= .225f)
			{
				if (moveSpeedMultiplier < animationMaxSpeed) moveSpeedMultiplier += Time.deltaTime * .5f;

				this.transform.position = Vector3.MoveTowards(this.transform.position, _target.position, moveSpeed * moveSpeedMultiplier * Time.deltaTime);
			}

			yield return Timing.WaitForOneFrame;
		}

		while (animationSpeed != 0)
		{
			animationSpeed = Mathf.MoveTowards(animationSpeed, 0f, acceleration * 1.25f * Time.deltaTime);

			animator.SetFloat(Define.MOVEMENT, animationSpeed);

			yield return Timing.WaitForOneFrame;
		}
	}

	public void Stop() => Move(this.transform);
	



	public void PlayMotion(string _parameter, bool _isLoop = false, float _blend = .25f, Action _action = null)
	{
		Timing.KillCoroutines(handle_playMotion);

		handle_playMotion = Timing.RunCoroutine(Co_PlayMotion(_parameter, _isLoop, _blend, _action));
	}

	IEnumerator<float> Co_PlayMotion(string _parameter, bool _isLoop = false, float _blend = .25f, Action _action = null)
	{
		animator.CrossFade(_parameter, _blend);

		yield return Timing.WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

		_action?.Invoke();

		if (_isLoop)
		{
			handle_playMotion = Timing.RunCoroutine(Co_PlayMotion(_parameter, _isLoop, _blend, _action));
		}
	}

	public void ExitMotion()
	{
		Timing.KillCoroutines(handle_playMotion);

		animator.Play("Motion");
	}
}
