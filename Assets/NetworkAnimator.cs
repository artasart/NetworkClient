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

		Animator animator;

		#endregion



		#region Initialize

		protected override void OnDestroy()
		{

		}

		protected override void Awake()
		{
			base.Awake();

			animator = GetComponentInChildren<Animator>();
		}

		protected override void Start()
		{
			base.Start();

			//handle_update = Timing.RunCoroutine(Co_Update());

			if (!isMine) GameClientManager.Instance.mainConnection.AddHandler(S_SET_ANIMATION);
		}

		private void Update()
		{
			if (isMine) return;

			if (isRecieved)
			{
				animator.SetFloat(Define.MOVEMENT, Mathf.Lerp(animator.GetFloat(Define.MOVEMENT), movement, lerpSpeed * Time.deltaTime));
				animator.SetInteger(Define.JUMP, jump);
				animator.SetBool(Define.SIT, sit);
			}
		}

		private IEnumerator<float> Co_Update()
		{
			if (!isMine) yield break;

			string prev = string.Empty;

			while (true)
			{
				var current = GetParameters();

				yield return Timing.WaitForSeconds(interval);

				if (!Equals(current, prev))
				{
					C_SET_ANIMATION();
				}

				prev = current.ToString();
			}
		}

		#endregion

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
			if (_packet.GameObjectId != objectId) return;

			foreach (var item in _packet.Params)
			{
				switch(item.Key)
				{
					case Define.MOVEMENT:
						//animator.SetFloat(Define.MOVEMENT, item.Value.FloatParam);
						movement = item.Value.FloatParam;
						break;

					case Define.JUMP:
						//animator.SetInteger(Define.JUMP, item.Value.IntParam);
						jump = item.Value.IntParam;
						break;

					case Define.SIT:
						//animator.SetBool(Define.SIT, item.Value.BoolParam);
						sit = item.Value.BoolParam;
						break;
				}
			}

			StringBuilder builder = new StringBuilder();

			builder.Append(movement.ToString("N4"))
				   .Append(jump)
				   .Append(sit);

			parameters = builder.ToString();

			isRecieved = true;
		}
		string parameters;

		float movement;
		int jump;
		bool sit;

		private string GetParameters()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(animator.GetFloat(Define.MOVEMENT).ToString("N4"))
				   .Append(animator.GetInteger(Define.JUMP))
				   .Append(animator.GetBool(Define.SIT));

			return builder.ToString();
		}
	}
}