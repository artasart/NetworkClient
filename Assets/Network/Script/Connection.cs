using Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;

namespace FrameWork.Network
{
	public class Connection
	{
		public string ConnectionId { get; set; }

		public ServerSession session;

		public PacketQueue packetQueue = new PacketQueue();
        public PacketHandler packetHandler = new PacketHandler();

        public Action onConnected;
		public Action onDisconnected;

		public Connection(string connectionId)
		{
			ConnectionId = connectionId;

            session = new ServerSession();

			session.callback_connect += OnConnected;
            session.callback_disconnect += OnDisConnected;
			session.callback_received += OnRecv;
		}

        private void OnConnected()
        {
			UnityEngine.Debug.Log("Connected");
            AsyncPacketUpdate().Forget();
        }

        private void OnDisConnected()
        {
        }

		//글로벌로 변경할 것
		RealtimePacket realtimePacket = new RealtimePacket();

		private void OnRecv(ArraySegment<byte> buffer)
		{
			realtimePacket.OnRecvPacket(buffer, packetQueue);
        }

		public async UniTaskVoid AsyncPacketUpdate()
		{
			while (true)
			{
                var packets = packetQueue.PopAll();

                for (var i = 0; i < packets.Count; i++)
                {
                    packetHandler.
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.02f));
			}
		}

		public void Enter() => C_ENTER();

		protected virtual void C_ENTER()
		{
			C_ENTER packet = new C_ENTER();

		}

		protected virtual void S_ENTER(S_ENTER _packet)
		{
			S_ENTER packet = _packet as S_ENTER;
		}


		public void ReEnter() => C_REENTER();

		protected virtual void C_REENTER()
		{
			C_REENTER packet = new C_REENTER();
		}

		protected void S_DISCONNECT(IMessage _packet)
		{
			S_DISCONNECT packet = _packet as S_DISCONNECT;
		}

		public void Clear()
		{
			session.Disconnect();
			//packetManager.Clear();

			RemoveHandler();
		}

		public void Leave()
		{
			C_LEAVE packet = new C_LEAVE();
		}
	}
}
