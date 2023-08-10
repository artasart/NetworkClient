using Framework.Network;
using Protocol;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

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
            AddHandler(OnAddGameObject);
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
            Debug.Log("Dummy Instantiate Success!");

            myGameObjectId = pkt.GameObjectId;

            //StopListening();
        }

        public void OnAddGameObject( S_ADD_GAME_OBJECT _packet )
        {
            Debug.Log("on add gameobject from dummyconnection");
        }

        public void StopListening()
        {
            Debug.Log("StopListening");

            cts.Cancel();

            session.receivedHandler -= _OnRecv;
        }
    }
}
