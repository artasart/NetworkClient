using System.Collections.Generic;
using Framework.Network;
using MEC;
using Protocol;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
	public class NetworkTransform : NetworkComponent
	{
		#region Members

		Vector3 position;
		Quaternion rotation;

		#endregion



		#region Initialize

		protected override void OnDestroy()
		{
			base.OnDestroy();
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
			if (isMine) return;

			var lerpPosition = Vector3.Lerp(this.transform.position, position, lerpSpeed * Time.deltaTime);
			var lerpRotation = Quaternion.Lerp(this.transform.rotation, rotation, lerpSpeed * Time.deltaTime);

			this.transform.position = lerpPosition;
			this.transform.rotation = lerpRotation;
		}

		private IEnumerator<float> Co_Update()
		{
			if (!isMine) yield break;

			Vector3 prev = Vector3.zero;

			while (true)
			{
				var current = this.transform.position;

				yield return Timing.WaitForSeconds(interval);

				if (Vector3.Distance(current, prev) > 0.001f)
				{
					C_SET_TRANSFORM();
				}

				prev = current;
			}
		}

		#endregion



		#region Core Methods

		private void C_SET_TRANSFORM()
		{
			C_SET_TRANSFORM packet = new()
			{
				GameObjectId = objectId
			};

			packet.Position = NetworkUtils.UnityVector3ToProtocolVector3(this.transform.position);
			packet.Rotation = NetworkUtils.UnityVector3ToProtocolVector3(this.transform.eulerAngles);

			GameClientManager.Instance.mainConnection.Send(PacketManager.MakeSendBuffer(packet));
		}

		private void S_SET_TRANSFORM(S_SET_TRANSFORM _packet)
		{
			if (_packet.GameObjectId != objectId) return;

			position = NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Position);
			rotation = NetworkUtils.ProtocolVector3ToUnityQuaternion(_packet.Rotation);
		}

		#endregion
	}
}