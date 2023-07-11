using System;
using System.Collections.Generic;
using Framework.Network;
using Google.Protobuf;
using MEC;
using Protocol;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
	public class NetworkTransform : NetworkComponent
	{
		#region Members

		float lerpSpeed = 10f;

		Vector3 position;
		Quaternion rotation;

		CoroutineHandle handle_update;

		#endregion



		#region Initialize

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Timing.KillCoroutines(handle_update);
		}

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();

			position = this.transform.position;
			rotation = this.transform.rotation;

			handle_update = Timing.RunCoroutine(Co_Update());

			if (!isMine) GameClientManager.Instance.mainConnection.AddHandler(S_SET_TRANSFORM);
		}

		private void Update()
		{
			//var lerpPosition = Vector3.Lerp(this.transform.position, position, lerpSpeed * Time.deltaTime);
			//var lerpRotation = Quaternion.Lerp(this.transform.rotation, rotation, lerpSpeed * Time.deltaTime);

			//Vector3 moveDir = (position - this.transform.position).normalized;
			//float moveDistance = Vector3.Distance(position, this.transform.position);
			//float moveSpeed = moveDistance / (lerpSpeed * Time.deltaTime);

			//this.transform.position = lerpPosition;
			//this.transform.rotation = lerpRotation;
		}

		private IEnumerator<float> Co_Update()
		{
			if (!isMine) yield break;

			while (true)
			{
				var position = this.transform.position;

				yield return Timing.WaitForOneFrame;

				if (!Equals(this.transform.position, position))
				{
					C_SET_TRANSFORM();
				}
			}
		}

		#endregion



		#region Core Methods

		private void C_SET_TRANSFORM()
		{
			C_SET_TRANSFORM C_SET_TRANSFORM = new()
			{
				GameObjectId = objectId
			};

			C_SET_TRANSFORM.Position = NetworkUtils.UnityVector3ToProtocolVector3(this.transform.position);
			C_SET_TRANSFORM.Rotation = NetworkUtils.UnityVector3ToProtocolVector3(this.transform.eulerAngles);

			GameClientManager.Instance.mainConnection.Send(PacketManager.MakeSendBuffer(C_SET_TRANSFORM));

			Debug.Log("C_SET_TRANSFORM");
		}

		private void S_SET_TRANSFORM(S_SET_TRANSFORM _packet)
		{
			position = NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Position);
			rotation = NetworkUtils.ProtocolVector3ToUnityQuaternion(_packet.Rotation);

			Debug.Log("S_SET_TRANSFORM : " + position + " " + rotation);
		}

		#endregion
	}
}