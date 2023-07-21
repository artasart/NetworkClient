using System.Collections.Generic;
using System.Diagnostics;
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

		float interval = 0.1f;
        int totalStep = 6;

		float x_velocity;
		float y_velocity;
		float z_velocity;

		Stopwatch stopwatch;

		#endregion

		#region Initialize

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Timing.KillCoroutines("Co_Test");
		}

		protected void Awake()
		{
			//base.Awake();
		}

		protected void Start()
		{
            stopwatch = new();

            if (isMine)
			{
				Timing.RunCoroutine(Co_Update());
                Timing.RunCoroutine(UpdateVelocity());
			}
			else
			{
                connection.AddHandler(S_SET_TRANSFORM);
            }
		}

		private IEnumerator<float> Co_Update()
		{
            var prev = new Vector3(x_velocity, y_velocity, z_velocity);
			var current = new Vector3();

            while (true)
			{
				current.x = x_velocity;
				current.y = y_velocity;
				current.z = z_velocity;

				if (prev != current)
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

        CoroutineHandle updateTransformHandler;

		private void S_SET_TRANSFORM(S_SET_TRANSFORM _packet)
		{
			if (_packet.GameObjectId != objectId) return;

            var timeGap = connection.calcuatedServerTime - _packet.Timestamp + interval * 1000;
			var predictedPosition = NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Position) + NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Velocity) * timeGap;

            if (isRunning)
			{
                Timing.KillCoroutines(updateTransformHandler);
				isRunning = false;
            }

            updateTransformHandler = Timing.RunCoroutine(UpdateTransform(predictedPosition, NetworkUtils.ProtocolVector3ToUnityQuaternion(_packet.Rotation)));
        }

		bool isRunning = false;

        private IEnumerator<float> UpdateTransform(Vector3 newPosition, Quaternion newRotation)
        {
            isRunning = true;

            var prevPosition = this.transform.position;
            var prevRotation = this.transform.rotation;

            stopwatch.Reset();
            stopwatch.Start();

            for (int currentStep = 1; currentStep <= totalStep; currentStep++)
            {
                this.transform.position = Vector3.Lerp(prevPosition, newPosition, (float)currentStep / totalStep);
                this.transform.rotation = Quaternion.Lerp(prevRotation, newRotation, (float)currentStep / totalStep);

                yield return Timing.WaitForSeconds((float)(interval * currentStep / (float)totalStep) - (float)stopwatch.Elapsed.TotalSeconds);
            }

            stopwatch.Stop();

            isRunning = false;
        }

        #endregion
    }
}