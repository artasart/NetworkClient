using Cysharp.Threading.Tasks;
using Protocol;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Framework.Network
{
    public class DummyClient : Connection
    {
        public string ClientId { get; set; }

        private int myGameObjectId = -1;

        public DummyClient()
        {
            AddHandler(OnEnter);
            AddHandler(OnInstantiateGameObject);
        }

        public void OnEnter( S_ENTER pkt )
        {
            if (pkt.Result != "SUCCESS")
            {
                Debug.Log(pkt.Result);
                return;
            }

            {
                C_INSTANTIATE_GAME_OBJECT packet = new();

                Protocol.Vector3 position = new()
                {
                    X = 0f,
                    Y = 0f,
                    Z = 0f
                };
                packet.Position = position;

                Protocol.Vector3 rotation = new()
                {
                    X = 0f,
                    Y = 0f,
                    Z = 0f
                };
                packet.Rotation = rotation;

                packet.PrefabName = "MarkerMan";

                Send(PacketManager.MakeSendBuffer(packet));
            }
        }

        public void OnInstantiateGameObject( S_INSTANTIATE_GAME_OBJECT pkt )
        {
            myGameObjectId = pkt.GameObjectId;

            StopListening();

            UpdateTransform().Forget();
        }

        public void StopListening()
        {
            Debug.Log("StopListening");

            cts.Cancel();

            session.receivedHandler -= _OnRecv;
        }

        private async UniTaskVoid UpdateTransform()
        {
            while(GameClientManager.Instance.Client == null && state == ConnectionState.NORMAL)
            {
                await UniTask.Delay(1000);
            }

            Protocol.Vector3 Position = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };
            Protocol.Vector3 Rotation = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };

            C_SET_TRANSFORM packet = new()
            {
                GameObjectId = myGameObjectId,
                Velocity = new Protocol.Vector3
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                },
                AngularVelocity = new Protocol.Vector3
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                }
            };

            packet.Position = Position;
            packet.Rotation = Rotation;

            while (state == ConnectionState.NORMAL)
            {
                Position.X += Random.Range(-0.01f, 0.01f);
                Position.Z += Random.Range(-0.01f, 0.01f);

                packet.Timestamp = GameClientManager.Instance.Client.calcuatedServerTime;

                Send(PacketManager.MakeSendBuffer(packet));

                await UniTask.Delay(50);
            }
        }
    }
}
