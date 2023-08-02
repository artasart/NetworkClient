using Framework.Network;
using MEC;
using Protocol;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
    public class NetworkTransform : NetworkComponent
    {
        private readonly float interval = 0.05f;
        private readonly int totalStep = 3;
        private Vector3 velocity;
        private Stopwatch stopwatch;
        private CoroutineHandle updateTransform;
        private CoroutineHandle updateVelocity;

        protected void Start()
        {
            stopwatch = new();

            if (isMine)
            {
                updateTransform = Timing.RunCoroutine(UpdateTransform());
                updateVelocity = Timing.RunCoroutine(UpdateVelocity());
            }
            else
            {
                Client.AddHandler(S_SET_TRANSFORM);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ = Timing.KillCoroutines(updateTransform);

            if (isMine)
            {
                _ = Timing.KillCoroutines(updateVelocity);
            }
            else
            {
                Client.RemoveHandler(S_SET_TRANSFORM);
            }
        }

        private IEnumerator<float> UpdateVelocity()
        {
            Vector3 prevPos = transform.position;
            Vector3 currentPos;

            while (true)
            {
                currentPos = transform.position;

                if (currentPos != prevPos)
                {
                    velocity.x = (currentPos.x - prevPos.x) / (Time.deltaTime * 1000);
                    velocity.y = (currentPos.y - prevPos.y) / (Time.deltaTime * 1000);
                    velocity.z = (currentPos.z - prevPos.z) / (Time.deltaTime * 1000);

                    prevPos = currentPos;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> UpdateTransform()
        {
            Vector3 prev = velocity;
            Vector3 current = new();

            while (true)
            {
                current.x = velocity.x;
                current.y = velocity.y;
                current.z = velocity.z;

                if (prev != current)
                {
                    C_SET_TRANSFORM();
                    prev = current;
                }

                yield return Timing.WaitForSeconds(interval);
            }
        }

        private void C_SET_TRANSFORM()
        {
            C_SET_TRANSFORM packet = new()
            {
                GameObjectId = objectId,
                Timestamp = Client.calcuatedServerTime,
                Position = NetworkUtils.UnityVector3ToProtocolVector3(transform.position),
                Rotation = NetworkUtils.UnityVector3ToProtocolVector3(transform.eulerAngles),
                Velocity = new Protocol.Vector3
                {
                    X = velocity.x,
                    Y = velocity.y,
                    Z = velocity.z
                }
            };

            Client.Send(PacketManager.MakeSendBuffer(packet));
        }

        private void S_SET_TRANSFORM( S_SET_TRANSFORM packet )
        {
            if (packet.GameObjectId != objectId)
            {
                return;
            }

            float timeGap = Client.calcuatedServerTime - packet.Timestamp + (interval * 1000);

            Vector3 predictedPosition = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position) +
                (NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity) * timeGap);

            if (updateTransform.IsRunning)
            {
                _ = Timing.KillCoroutines(updateTransform);
            }

            updateTransform = Timing.RunCoroutine(
                UpdateTransform(
                    predictedPosition,
                    Quaternion.Euler(NetworkUtils.ProtocolVector3ToUnityVector3(packet.Rotation))
                    )
                );
        }

        private IEnumerator<float> UpdateTransform( Vector3 newPosition, Quaternion newRotation )
        {
            Vector3 prevPosition = transform.position;
            Quaternion prevRotation = transform.rotation;

            stopwatch.Reset();
            stopwatch.Start();

            for (int currentStep = 1; currentStep <= totalStep; currentStep++)
            {
                transform.position = Vector3.Lerp(prevPosition, newPosition, (float)currentStep / totalStep);
                transform.rotation = Quaternion.Lerp(prevRotation, newRotation, (float)currentStep / totalStep);

                yield return Timing.WaitForSeconds((float)(interval * currentStep / totalStep) - (float)stopwatch.Elapsed.TotalSeconds);
            }

            stopwatch.Stop();
        }
    }
}