using Protocol;
using Google.Protobuf;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

			packetHandler.AddHandler(Handle_S_ENTER);
		}

        private void OnConnected()
        {
			UnityEngine.Debug.Log("Connected");
            AsyncPacketUpdate().Forget();

            C_ENTER packet = new C_ENTER();
			packet.ClientId = ConnectionId;
			session.Send(packet);
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
            Action<IMessage> handler;

            while (true)
			{
                var packets = packetQueue.PopAll();

                for (var i = 0; i < packets.Count; i++)
                {
					var packet = packets[i];
					packetHandler.Handlers.TryGetValue(packet.Id, out handler);
                    handler?.Invoke(packet.Message);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.02f));
			}
		}

		void Handle_S_ENTER(Protocol.S_ENTER enter)
		{
			Debug.Log(enter.Result);
		}
	}
}
