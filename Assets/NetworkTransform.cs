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

		Queue<S_SET_TRANSFORM> queue = new Queue<S_SET_TRANSFORM>();

		float x_velocity;
		float y_velocity;
		float z_velocity;

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

			if (isMine)
			{
				handle_update = Timing.RunCoroutine(Co_Update());
                Timing.RunCoroutine(UpdateVelocity());
			}
			else
			{
                connection.AddHandler(S_SET_TRANSFORM);
            }
		}

		private IEnumerator<float> Co_Update()
		{
            var prev = this.transform.position;

            while (true)
			{
				var current = this.transform.position;

				if (Vector3.Distance(current, prev) > 0.001f)
				{
					C_SET_TRANSFORM();
                    prev = current;
                }

                yield return Timing.WaitForSeconds(interval);
            }
		}

        private IEnumerator<float> UpdateVelocity()
        {
            var prev = this.transform.position;

            while (true)
            {
                var current = this.transform.position;

				x_velocity = (current.x - prev.x) / (Time.deltaTime * 1000);
				y_velocity = (current.y - prev.y) / (Time.deltaTime * 1000);
                z_velocity = (current.z - prev.z) / (Time.deltaTime * 1000);

				//print x, y, z velocity
				//print(x_velocity + ", " + y_velocity + ", " + z_velocity);

                prev = current;

                yield return Timing.WaitForOneFrame;
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

			packet.Timestamp = connection.calcuatedServerTime;
			packet.Position = NetworkUtils.UnityVector3ToProtocolVector3(this.transform.position);
			packet.Rotation = NetworkUtils.UnityVector3ToProtocolVector3(this.transform.eulerAngles);
			packet.Velocity = new Protocol.Vector3 
			{
				X = x_velocity,
                Y = y_velocity,
                Z = z_velocity
			};

            connection.Send(PacketManager.MakeSendBuffer(packet));
		}

		private void S_SET_TRANSFORM(S_SET_TRANSFORM _packet)
		{
			if (_packet.GameObjectId != objectId) return;

			var timeGap = connection.calcuatedServerTime - _packet.Timestamp;
			//predict position by position + velocity * timeGap
			var predictedPosition = NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Position) + NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Velocity) * timeGap;
			//print predicted position
			print("predicted Position : " + predictedPosition);

			//queue.Enqueue(_packet);

			//if (!isRunning) Timing.RunCoroutine(Co_SET_TRANSFORM(), "Co_SET_TRANSFORM");
		}

		private IEnumerator<float> Co_SET_TRANSFORM()
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