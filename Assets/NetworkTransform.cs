﻿using Framework.Network;
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
        #region Members

        private readonly float interval = 0.05f;
        private readonly int totalStep = 3;
        private Vector3 velocity;
        private Stopwatch stopwatch;

        CoroutineHandle setTransformHandler;
        CoroutineHandle updateVelocityHandler;

        #endregion

        #region Initialize

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (isMine)
            {
                Timing.KillCoroutines(setTransformHandler);
                Timing.KillCoroutines(updateVelocityHandler);
            }
            else
            {
                connection.RemoveHandler(S_SET_TRANSFORM);
            }
        }

        protected void Start()
        {
            stopwatch = new();

            if (isMine)
            {
                setTransformHandler = Timing.RunCoroutine(SetTransform());
                updateVelocityHandler =  Timing.RunCoroutine(UpdateVelocity());
            }
            else
            {
                connection.AddHandler(S_SET_TRANSFORM);
            }
        }

        private IEnumerator<float> SetTransform()
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

        private IEnumerator<float> UpdateVelocity()
        {
            Vector3 prevPos = transform.position;
            Vector3 currentPos;

            while (true)
            {
                currentPos = transform.position;

                if(currentPos != prevPos)
                {
                    velocity.x = (currentPos.x - prevPos.x) / (Time.deltaTime * 1000);
                    velocity.y = (currentPos.y - prevPos.y) / (Time.deltaTime * 1000);
                    velocity.z = (currentPos.z - prevPos.z) / (Time.deltaTime * 1000);

                    prevPos = currentPos;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        #endregion

        #region Core Methods

        private void C_SET_TRANSFORM()
        {
            C_SET_TRANSFORM packet = new()
            {
                GameObjectId = objectId,
                Timestamp = connection.calcuatedServerTime,
                Position = NetworkUtils.UnityVector3ToProtocolVector3(transform.position),
                Rotation = NetworkUtils.UnityVector3ToProtocolVector3(transform.eulerAngles),
                Velocity = new Protocol.Vector3
                {
                    X = velocity.x,
                    Y = velocity.y,
                    Z = velocity.z
                }
            };

            connection.Send(PacketManager.MakeSendBuffer(packet));
        }

        private CoroutineHandle updateTransformHandler;

        private void S_SET_TRANSFORM( S_SET_TRANSFORM _packet )
        {
            if (_packet.GameObjectId != objectId)
            {
                return;
            }

            //print calcuatedServerTime
            print("set transform arrived : " + connection.calcuatedServerTime);

            float timeGap = connection.calcuatedServerTime - _packet.Timestamp + (interval * 1000);

            Vector3 predictedPosition = NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Position) +
                (NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Velocity) * timeGap);

            if (updateTransformHandler.IsRunning)
                Timing.KillCoroutines(updateTransformHandler);

            updateTransformHandler = Timing.RunCoroutine(
                UpdateTransform(
                    predictedPosition,
                    Quaternion.Euler(NetworkUtils.ProtocolVector3ToUnityVector3(_packet.Rotation))
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

        #endregion
    }
}