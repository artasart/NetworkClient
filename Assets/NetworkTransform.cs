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
        private Vector3 angularVelocity;
        private Stopwatch stopwatch;
        private CoroutineHandle updateTransform;
        private CoroutineHandle updateVelocity;
        private CoroutineHandle updateAngularVelocity;

        protected void Start()
        {
            velocity = new();
            angularVelocity = new();

            stopwatch = new();

            if (isMine)
            {
                updateTransform = Timing.RunCoroutine(UpdateTransform());
                updateVelocity = Timing.RunCoroutine(UpdateVelocity());
                updateAngularVelocity = Timing.RunCoroutine(UpdateAngularVelocity());
            }
            else
            {
                Client.AddHandler(S_SET_TRANSFORM);
                updateTransform = Timing.RunCoroutine(RemoteUpdateTransform());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ = Timing.KillCoroutines(updateTransform);

            if (isMine)
            {
                _ = Timing.KillCoroutines(updateVelocity);
                _ = Timing.KillCoroutines(updateAngularVelocity);
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
            float prevDeltaTime = Time.deltaTime;

            while (true)
            {
                currentPos = transform.position;
                velocity = (currentPos - prevPos) / (prevDeltaTime * 1000);

                prevPos = currentPos;
                prevDeltaTime = Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> UpdateAngularVelocity()
        {
            Quaternion prevRot = transform.rotation;
            Quaternion currentRot;
            float prevDeltaTime = Time.deltaTime;

            while (true)
            {
                currentRot = transform.rotation;

                Quaternion deltaRotation = currentRot * Quaternion.Inverse(prevRot);
                deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180)
                {
                    angle -= 360;
                }

                angularVelocity = angle * Mathf.Deg2Rad * axis.normalized / (prevDeltaTime * 1000);

                prevRot = currentRot;
                prevDeltaTime = Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> UpdateTransform()
        {
            Vector3 prevVelocity = velocity;
            Vector3 prevAngularVelocity = angularVelocity;

            while (true)
            {
                if (prevVelocity != velocity || prevAngularVelocity != angularVelocity)
                {
                    print("update transform");
                    C_SET_TRANSFORM();
                    prevVelocity = velocity;
                    prevAngularVelocity = angularVelocity;
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
                },
                AngularVelocity = new Protocol.Vector3
                {
                    X = angularVelocity.x,
                    Y = angularVelocity.y,
                    Z = angularVelocity.z
                }
            };

            Client.Send(PacketManager.MakeSendBuffer(packet));
        }

        private IEnumerator<float> RemoteUpdateTransform()
        {
            while(true)
            {
                print("remote update transfrom");

                transform.position += velocity * Time.deltaTime * 1000;
                transform.rotation *= Quaternion.AngleAxis(angularVelocity.magnitude * Time.deltaTime * 1000 * Mathf.Rad2Deg, angularVelocity.normalized);
                yield return Timing.WaitForOneFrame;
            }
        }

        private void S_SET_TRANSFORM( S_SET_TRANSFORM packet )
        {
            if (packet.GameObjectId != objectId)
            {
                return;
            }

            print("set transform");

            velocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity);
            angularVelocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.AngularVelocity);

            //float timeGap = Client.calcuatedServerTime - packet.Timestamp + (interval * 1000);
            float timeGap = Client.calcuatedServerTime - packet.Timestamp;

            Vector3 predictedPosition = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position) +
                (NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity) * timeGap);

            Quaternion predictedRotation =
                Quaternion.Euler(NetworkUtils.ProtocolVector3ToUnityVector3(packet.Rotation)) *
                Quaternion.AngleAxis(angularVelocity.magnitude * timeGap * Mathf.Rad2Deg, angularVelocity.normalized);

            if(Vector3.Distance(predictedPosition, transform.position) > 0.5f)
            {
                print("teleport");
                transform.position = predictedPosition;
                transform.rotation = predictedRotation;
            }
            //else
            //{

            //}

            //if (updateTransform.IsRunning)
            //{
            //    _ = Timing.KillCoroutines(updateTransform);
            //}

            //updateTransform = Timing.RunCoroutine(
            //    UpdateTransform(
            //        predictedPosition,
            //        predictedRotation
            //        )
            //    );
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