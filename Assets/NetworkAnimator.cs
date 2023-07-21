using System;
using System.Collections.Generic;
using System.Text;
using Framework.Network;
using MEC;
using Protocol;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkAnimator : NetworkComponent
	{
		#region Members

		float interval = 0.1f;

		Animator animator;

		Queue<S_SET_ANIMATION> queue = new Queue<S_SET_ANIMATION>();

		#endregion



		#region Initialize

		protected void Awake()
		{
			//base.Awake();

			animator = GetComponentInChildren<Animator>();
		}

		protected void Start()
		{

			if (!isMine) GameClientManager.Instance.mainConnection.AddHandler(S_SET_ANIMATION);
		}

		private IEnumerator<float> Co_Update()
		{
			if (!isMine || !isPlayer) yield break;

			string prev = string.Empty;

			while (true)
			{
				var current = GetParameters();

				yield return Timing.WaitForSeconds(interval);

				if (!Equals(current, prev) || animator.GetFloat(Define.MOVEMENT) >= Define.THRESHOLD_MOVEMENT)
				{
					C_SET_ANIMATION();
				}

				prev = current.ToString();
			}
		}

		#endregion



		#region Core Methods

		private void C_SET_ANIMATION()
		{
			C_SET_ANIMATION packet = new()
			{
				GameObjectId = objectId
			};

			AnimationParameter movement = new AnimationParameter();
			movement.FloatParam = animator.GetFloat(Define.MOVEMENT);
			packet.Params.Add(Define.MOVEMENT, movement);

			AnimationParameter jump = new AnimationParameter();
			jump.IntParam = animator.GetInteger(Define.JUMP);
			packet.Params.Add(Define.JUMP, jump);

			AnimationParameter sit = new AnimationParameter();
			sit.BoolParam = animator.GetBool(Define.SIT);
			packet.Params.Add(Define.SIT, sit);

			GameClientManager.Instance.mainConnection.Send(PacketManager.MakeSendBuffer(packet));
		}

		private void S_SET_ANIMATION(S_SET_ANIMATION _packet)
		{
			//if (_packet.GameObjectId != objectId) return;

			//queue.Enqueue(_packet);

			//if (!isRunning) Timing.RunCoroutine(Co_SET_ANIMATION(), "Co_SET_ANIMATION");
		}

		//private IEnumerator<float> Co_SET_ANIMATION()
		//{
		//	isRunning = true;

		//	yield return Timing.WaitForSeconds(interval * 3f);

		//	while (queue.Count > 0)
		//	{
		//		stopwatch.Reset();
		//		stopwatch.Start();

		//		var target = queue.Dequeue();
								
		//		animator.SetInteger(Define.JUMP, target.Params[Define.JUMP].IntParam);
		//		animator.SetBool(Define.SIT, target.Params[Define.SIT].BoolParam);

		//		foreach (var item in target.Params)
		//		{
		//			switch (item.Key)
		//			{
		//				case Define.MOVEMENT:
		//					for (int currentStep = 1; currentStep <= totalStep; currentStep++)
		//					{
		//						animator.SetFloat(Define.MOVEMENT, Mathf.Lerp(animator.GetFloat(Define.MOVEMENT), item.Value.FloatParam, (float)currentStep / totalStep));

		//						yield return Timing.WaitForSeconds((float)(interval * currentStep / (float)totalStep) - (float)stopwatch.Elapsed.TotalSeconds);
		//					}
		//					break;
		//			}
		//		}

		//		stopwatch.Stop();
		//	}

		//	isRunning = false;
		//}


		#endregion


		#region Basic Methods

		private string GetParameters()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(animator.GetFloat(Define.MOVEMENT).ToString("N4"))
				   .Append(animator.GetInteger(Define.JUMP))
				   .Append(animator.GetBool(Define.SIT));

			return builder.ToString();
		}

		#endregion
	}
}