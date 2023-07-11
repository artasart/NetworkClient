//using System;
//using System.Collections.Generic;
//using System.Text;
//using Google.Protobuf;
//using MEC;
//using Protocol;
//using UnityEngine;

//namespace FrameWork.Network
//{
//	public class NetworkAnimator : NetworkComponent
//	{
//		#region Members

//		Animator animator;

//		string[] animations;
//		string packetAnimation;
//		bool isRecieved = false;

//		float lerpSpeed = 10f;

//		CoroutineHandle handle_update;
//		CoroutineHandle handle_weight;

//		#endregion



//		#region Initialize

//		protected override void OnDestroy()
//		{
//			base.OnDestroy();

//			RemoveHandler();

//			Timing.KillCoroutines(handle_update);
//			Timing.KillCoroutines(handle_weight);
//		}

//		protected override void Awake()
//		{
//			base.Awake();

//			animator = this.transform.Search(Cons.AVATARPARTS).GetComponent<Animator>();
//		}

//		protected override void Start()
//		{
//			base.Start();

//			AddHandler();

//			handle_update = Timing.RunCoroutine(Co_Update());

//			packetAnimation = ParseAnimation();
//		}

//		private void Update()
//		{
//			if (isRecieved)
//			{
//				for (var i = 0; i < animator.parameters.Length; i++)
//				{
//					var parameter = animator.parameters[i];

//					switch (parameter.type)
//					{
//						case AnimatorControllerParameterType.Float:
//							var targetFloatValue = float.Parse(animations[i]);
//							var currentFloatValue = animator.GetFloat(parameter.name);
//							var lerpedFloatValue = Mathf.Lerp(currentFloatValue, targetFloatValue, lerpSpeed * Time.deltaTime);
//							animator.SetFloat(parameter.name, lerpedFloatValue);
//							break;

//						case AnimatorControllerParameterType.Int:
//							var targetIntValue = int.Parse(animations[i]);
//							var currentIntValue = animator.GetFloat(parameter.name);
//							var lerpedIntValue = Mathf.Lerp(currentIntValue, targetIntValue, lerpSpeed * Time.deltaTime);
//							animator.SetFloat(parameter.name, lerpedIntValue);
//							break;

//						case AnimatorControllerParameterType.Bool:
//							animator.SetBool(parameter.name, Convert.ToBoolean(int.Parse(animations[i])));
//							break;

//						case AnimatorControllerParameterType.Trigger:
//							animator.SetTrigger(parameter.name);
//							break;
//					}
//				}

//				if (Equals(packetAnimation, ParseAnimation()))
//				{
//					isRecieved = false;
//				}
//			}
//		}

//		private IEnumerator<float> Co_Update()
//		{
//			if (!isMine) yield break;

//			string prev = string.Empty;

//			while (true)
//			{
//				string animation = ParseAnimation();

//				if (prev != animation || animator.GetFloat(Cons.Movement) >= .001f)
//				{
//					C_BASE_SET_ANIMATION(animation);
//				}

//				prev = animation;

//				yield return Timing.WaitForOneFrame;
//			}
//		}

//		private void AddHandler()
//		{
//			if (!Single.RealTime) return;

//			if (!isMine)
//			{
//				RealtimeUtils.AddHandler(RealtimePacket.MsgId.PKT_S_BASE_SET_ANIMATION, this, S_BASE_ANIMATION);
//			}

//			RealtimeUtils.AddHandler(RealtimePacket.MsgId.PKT_S_BASE_SET_ANIMATION_ONCE, this, S_BASE_ANIMATION_ONCE);
//		}

//		public void RemoveHandler()
//		{
//			if (!Single.RealTime) return;

//			if (!isMine)
//			{
//				RealtimeUtils.RemoveHandler(RealtimePacket.MsgId.PKT_S_BASE_SET_ANIMATION, this);
//			}

//			RealtimeUtils.RemoveHandler(RealtimePacket.MsgId.PKT_S_BASE_SET_ANIMATION_ONCE, this);
//		}

//		#endregion



//		#region Core Methods

//		public void PlayAnimation(string _parameter, bool _loop = false, float _blend = .25f)
//		{
//			Play(_parameter, _blend);

//			C_BASE_ANIMATION_ONCE(_parameter, _loop, _blend);
//		}

//		public void Play(string _parameter, float _blend = .25f)
//		{
//			if (_blend == 0) animator.Play(_parameter, -1);

//			else animator.CrossFade(_parameter, _blend);
//		}

//		#endregion




//		#region Send

//		private void C_BASE_SET_ANIMATION(string _animation)
//		{
//			var packet = new C_BASE_SET_ANIMATION();

//			packet.ObjectId = objectId;
//			packet.AnimationId = animator.runtimeAnimatorController.name;
//			packet.Animation = _animation.Compress();

//			Single.RealTime.Send(packet);
//		}

//		private void C_BASE_ANIMATION_ONCE(string _parameter, bool _loop = false, float _blend = .25f)
//		{
//			var packet = new C_BASE_SET_ANIMATION_ONCE();

//			packet.ObjectId = this.objectId;
//			packet.AnimationId = _parameter;
//			packet.IsLoop = _loop;
//			packet.Blend = _blend;

//			Single.RealTime.Send(packet);
//		}

//		#endregion




//		#region Recieve

//		private void S_BASE_ANIMATION(PacketSession _session, IMessage _packet)
//		{
//			var packet = _packet as S_BASE_SET_ANIMATION;

//			if (packet.ObjectId != objectId) return;

//			packetAnimation = packet.Animation.Decompress().TrimEnd();
//			animations = packet.Animation.Decompress().TrimEnd().Split(' ');

//			isRecieved = true;
//		}

//		private void S_BASE_ANIMATION_ONCE(PacketSession _session, IMessage _packet)
//		{
//			if (isMine) return;

//			S_BASE_SET_ANIMATION_ONCE packet = _packet as S_BASE_SET_ANIMATION_ONCE;

//			if (packet != null && packet.ObjectId == objectId)
//			{
//				SetWeight(1f);

//				if (packet.Blend == 0)
//				{
//					animator.Play(packet.AnimationId);
//				}

//				else animator.CrossFade(packet.AnimationId, packet.Blend);

//				if (packet.IsLoop) return;

//				ResetWeight(packet.AnimationId, 0f);
//			}
//		}

//		#endregion




//		#region Basic Methods

//		private string ParseAnimation()
//		{
//			if (animator == null) return string.Empty;

//			StringBuilder builder = new StringBuilder();

//			for (var i = 0; i < animator.parameters.Length; i++)
//			{
//				var value = animator.parameters[i];

//				switch (value.type)
//				{
//					case AnimatorControllerParameterType.Float:
//						builder.Append($"{animator.GetFloat(value.name):0.0000}").Append(" ");
//						break;

//					case AnimatorControllerParameterType.Int:
//						builder.Append(animator.GetInteger(value.name)).Append(" ");
//						break;

//					case AnimatorControllerParameterType.Trigger:
//						builder.Append(value.name).Append(" ");
//						break;

//					case AnimatorControllerParameterType.Bool:
//						var parameter = animator.GetBool(animator.parameters[i].name) ? 1 : 0;
//						builder.Append(parameter).Append(" ");
//						break;
//				}
//			}

//			return builder.ToString().TrimEnd();
//		}


//		public void SetWeight(float _target = 0f)
//		{
//			handle_weight = Timing.RunCoroutine(Co_SetWeight(_target));
//		}

//		IEnumerator<float> Co_SetWeight(float _target = 0)
//		{
//			float value = animator.GetLayerWeight(Define.EmotionLayer);
//			float speed = 2.5f;

//			while (Mathf.Abs(value - _target) >= 0.001f)
//			{
//				if (animator == null) yield break;

//				value = Mathf.Lerp(value, _target, value += speed * Time.deltaTime);

//				animator.SetLayerWeight(Define.EmotionLayer, value);

//				yield return Timing.WaitForOneFrame;
//			}

//			animator.SetLayerWeight(Define.EmotionLayer, _target);
//		}


//		public void ResetWeight(string _animation, float _target = 0f)
//		{
//			if (_animation != Define.Action_Idle) return;

//			handle_weight = Timing.RunCoroutine(Co_ResetWeight(_animation, _target));
//		}

//		IEnumerator<float> Co_ResetWeight(string _animation, float _target = 0)
//		{
//			yield return Timing.WaitUntilTrue(() => animator.GetCurrentAnimatorStateInfo(Define.EmotionLayer).IsName(_animation));

//			float value = animator.GetLayerWeight(Define.EmotionLayer);
//			float lerpvalue = 0f;
//			float lerpspeed = 2.5f;

//			while (Mathf.Abs(value - _target) >= 0.001f)
//			{
//				value = Mathf.Lerp(value, _target, lerpvalue += lerpspeed * Time.deltaTime);

//				animator.SetLayerWeight(Define.EmotionLayer, value);

//				yield return Timing.WaitForOneFrame;
//			}

//			animator.SetLayerWeight(Define.EmotionLayer, _target);
//		}

//		#endregion
//	}
//}