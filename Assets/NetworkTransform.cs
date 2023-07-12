using System.Collections.Generic;
using Framework.Network;
using MEC;
using Protocol;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
	public class NetworkTransform : NetworkComponent
	{
		#region Members

		Vector3 position;
		Quaternion rotation;

		bool isRunning = false;

		#endregion



		#region Initialize

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Timing.KillCoroutines("Co_Test");
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

		Queue<S_SET_TRANSFORM> queue = new Queue<S_SET_TRANSFORM>();
		
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

			queue.Enqueue(_packet);

			if (!isRunning) Timing.RunCoroutine(Co_Test(), "Co_Test");
		}

		IEnumerator<float> Co_Test()
		{
			isRunning = true;

			yield return Timing.WaitForSeconds(interval * 3f);

			while (queue.Count > 0)
			{
				stopwatch.Reset();
				stopwatch.Start();

				var target = queue.Dequeue();

				for (int currentStep = 1; currentStep <= totalStep; currentStep++)
				{
					this.transform.position = Vector3.Lerp(this.transform.position, NetworkUtils.ProtocolVector3ToUnityVector3(target.Position), (float)currentStep / totalStep);
					this.transform.rotation = Quaternion.Lerp(this.transform.rotation, NetworkUtils.ProtocolVector3ToUnityQuaternion(target.Rotation), (float)currentStep / totalStep);

					yield return Timing.WaitForSeconds((float)(interval * currentStep / (float)totalStep) - (float)stopwatch.Elapsed.TotalSeconds);
				}

				stopwatch.Stop();
			}

			isRunning = false;
		}

		#endregion
	}
}