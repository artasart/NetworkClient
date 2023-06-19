using Protocol;
using Google.Protobuf;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace FrameWork.Network
{
	public class Connection
	{
		public string ClientId { get; set; }

		public ServerSession session;

		public PacketQueue packetQueue = new();
        public PacketHandler packetHandler = new();

        public Action disconnectedHandler;

        private RealtimePacket rtp;

        private DateTime lastMessageSent;

        private bool isConnected;

        public Connection(string connectionId, RealtimePacket _rtp)
		{
			ClientId = connectionId;

            rtp = _rtp;

            lastMessageSent = new DateTime(1970, 1, 1);

            isConnected = false;

            session = new ServerSession();

			session.callback_connect += OnConnected;
            session.callback_disconnect += OnDisConnected;
			session.callback_received += OnRecv;
        }

        private void OnConnected()
        {
			UnityEngine.Debug.Log("Connected");

            isConnected = true;

            AsyncPacketUpdate().Forget();
            HeartBeat().Forget();

            C_ENTER packet = new C_ENTER();
			packet.ClientId = ClientId;

			session.Send(packet);
        }

        private void OnDisConnected()
        {
            isConnected = false;
        }

        private void HandleDisconnect()
        {
            ClientManager.Instance.DestroyDummy(ClientId);
        }

		private void OnRecv(ArraySegment<byte> buffer)
		{
            rtp.OnRecvPacket(buffer, packetQueue);
        }

        public void Send(IMessage pkt)
        {
            lastMessageSent = DateTime.UtcNow;
            session.Send(pkt);
        }

		public async UniTaskVoid AsyncPacketUpdate()
		{
            Action<IMessage> handler;

            while (isConnected)
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

            disconnectedHandler?.Invoke();
		}

        public async UniTaskVoid HeartBeat()
        {
            Protocol.C_HEARTBEAT heartbeat = new Protocol.C_HEARTBEAT();

            while (isConnected)
            {
                if ((long)(DateTime.UtcNow - lastMessageSent).TotalSeconds > 5)
                {
                    Send(heartbeat);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(5));
            }
        }

        void Handle_S_ENTER(Protocol.S_ENTER enter)
		{
			Debug.Log("ENTER : " + enter.Result);

            var packet = new C_INSTANTIATE_GAME_OBJECT();

            var position = new Protocol.Vector3();
            position.X = 0f;
            position.Y = 0f;
            position.Z = 0f;

            var rotation = new Protocol.Vector3();
            rotation.X = 0f;
            rotation.Y = 0f;
            rotation.Z = 0f;

            packet.Position = position;
            packet.Rotation = rotation;

            Send(packet);
        }

        void Handle_S_ADDCLIENT(Protocol.S_ADD_CLIENT packet)
        {
            Debug.Log("ADD CLIENT.");
        }

        void Handle_S_INSTANTIATE(Protocol.S_INSTANTIATE_GAME_OBJECT packet)
        {
            Debug.Log("INSTANTIATE OBJECT.");

            ClientManager.Instance.CreateDummy(ClientId);
        }
    }
}
