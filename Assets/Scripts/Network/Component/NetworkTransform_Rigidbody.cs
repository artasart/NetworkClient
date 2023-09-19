using Framework.Network;
using MEC;
using Protocol;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
    public class NetworkTransform_Rigidbody : NetworkComponent
    {
        private readonly float interval = 0.05f;
        private readonly float hardsnapThreshold = 3f;

        private CoroutineHandle updateTransform;

        private new Rigidbody rigidbody;

        protected void Start()
        {
            rigidbody = GetComponent<Rigidbody>();

            updateTransform = Timing.RunCoroutine(UpdateTransform());
            Client.packetHandler.AddHandler(S_SET_TRANSFORM);

            Client.packetHandler.AddHandler(OnOwnerChanged);
        }

        private void OnOwnerChanged( Protocol.S_SET_GAME_OBJECT_OWNER pkt )
        {
            if (pkt.GameObjectId != id)
            {
                return;
            }

            isMine = pkt.OwnerId == Client.ClientId;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (isMine)
            {
                _ = Timing.KillCoroutines(updateTransform);
            }
            else
            {
                Client.packetHandler.RemoveHandler(S_SET_TRANSFORM);
            }
        }

        private IEnumerator<float> UpdateTransform()
        {
            Vector3 prevVelocity = rigidbody.velocity;
            Vector3 prevAngularVelocity = rigidbody.angularVelocity;

            float delTime = 0;

            while (true)
            {
                if (isMine == false)
                {
                    yield return Timing.WaitForOneFrame;
                }

                delTime += Time.deltaTime;
                if (delTime > interval)
                {
                    delTime -= interval;

                    if (prevVelocity != rigidbody.velocity || prevAngularVelocity != rigidbody.angularVelocity)
                    {
                        C_SET_TRANSFORM();
                        prevVelocity = rigidbody.velocity;
                        prevAngularVelocity = rigidbody.angularVelocity;
                    }
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private void C_SET_TRANSFORM()
        {
            C_SET_TRANSFORM packet = new()
            {
                GameObjectId = id,
                Timestamp = Client.calcuatedServerTime,
                Position = NetworkUtils.UnityVector3ToProtocolVector3(rigidbody.transform.position),
                Rotation = NetworkUtils.UnityVector3ToProtocolVector3(rigidbody.transform.eulerAngles),
                Velocity = NetworkUtils.UnityVector3ToProtocolVector3(rigidbody.velocity),
                AngularVelocity = NetworkUtils.UnityVector3ToProtocolVector3(rigidbody.angularVelocity)
            };

            Client.Send(PacketManager.MakeSendBuffer(packet));
        }

        private void S_SET_TRANSFORM( S_SET_TRANSFORM packet )
        {
            if (isMine)
            {
                return;
            }

            if (packet.GameObjectId != id)
            {
                return;
            }

            float timeGap;

            Vector3 packetPosition = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position);
            rigidbody.velocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity);
            Vector3 predictedPosition;

            Quaternion packetRotation = NetworkUtils.ProtocolVector3ToUnityQuaternion(packet.Rotation);
            rigidbody.angularVelocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.AngularVelocity);
            Quaternion predictedRotation;

            timeGap = (Client.calcuatedServerTime - packet.Timestamp) / 1000f;

            predictedRotation = packetRotation * Quaternion.AngleAxis(rigidbody.angularVelocity.magnitude * timeGap * Mathf.Rad2Deg, rigidbody.angularVelocity.normalized);
            rigidbody.transform.rotation = predictedRotation;

            if (2.0f * Mathf.Acos(Mathf.Clamp((transform.rotation * Quaternion.Inverse(predictedRotation)).w, -1.0f, 1.0f)) * Mathf.Rad2Deg > 3.0f)
            {
                rigidbody.transform.rotation = predictedRotation;
            }

            predictedPosition = packetPosition + (rigidbody.velocity * timeGap);
            float distance = Vector3.Distance(predictedPosition, transform.position);

            if (distance > hardsnapThreshold)
            {
                rigidbody.transform.position = predictedPosition;
            }
        }
    }
}