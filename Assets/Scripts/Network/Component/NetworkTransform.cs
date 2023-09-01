using Framework.Network;
using MEC;
using Protocol;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
    public class NetworkTransform : NetworkComponent
    {
        private readonly float interval = 0.05f;

        private CoroutineHandle updateTransform;

        private CharacterController controller;

        protected void Start()
        {
            controller = GetComponent<CharacterController>();

            if (isMine)
            {
                updateTransform = Timing.RunCoroutine(UpdateTransform());
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

            if (!isMine)
            {
                Client.RemoveHandler(S_SET_TRANSFORM);
            }
        }

        private IEnumerator<float> UpdateTransform()
        {
            Vector3 prevPosition = transform.position;
            Vector3 prevRotation = transform.eulerAngles;

            float delTime = 0;

            while (true)
            {
                delTime += Time.deltaTime;
                if (delTime > interval)
                {
                    delTime -= interval;

                    if (prevPosition != transform.position || prevRotation != transform.eulerAngles)
                    {
                        C_SET_TRANSFORM();
                        prevPosition = transform.position;
                        prevRotation = transform.eulerAngles;
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
                Position = NetworkUtils.UnityVector3ToProtocolVector3(transform.position),
                Rotation = NetworkUtils.UnityVector3ToProtocolVector3(transform.eulerAngles),
            };

            Client.Send(PacketManager.MakeSendBuffer(packet));
        }

        private void S_SET_TRANSFORM( S_SET_TRANSFORM packet )
        {
            if (packet.GameObjectId != id)
            {
                return;
            }

            if (updateTransform.IsRunning)
            {
                _ = Timing.KillCoroutines(updateTransform);
            }

            updateTransform = Timing.RunCoroutine(
                RemoteUpdateTransform(
                        NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position),
                        NetworkUtils.ProtocolVector3ToUnityQuaternion(packet.Rotation)
                    )
                );
        }

        private IEnumerator<float> RemoteUpdateTransform( Vector3 endPosition, Quaternion endRotation )
        {
            float delTime = 0;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            while (delTime < interval)
            {
                delTime += Time.deltaTime;

                _ = controller.Move(Vector3.Lerp(startPosition, endPosition, delTime / interval) - transform.position);
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, delTime / interval);

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}