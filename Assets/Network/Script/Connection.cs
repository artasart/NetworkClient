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
		public string ConnectionId { get; set; }

		public ServerSession session;

		public PacketQueue packetQueue = new PacketQueue();
        public PacketHandler packetHandler = new PacketHandler();

        public Action onConnected;
		public Action onDisconnected;

        private CancellationTokenSource cts;

        private RealtimePacket rtp;

        private DateTime lastMessageSent;

        public Connection(string connectionId, RealtimePacket _rtp)
		{
			ConnectionId = connectionId;

            rtp = _rtp;

            lastMessageSent = new DateTime(1970, 1, 1);

            session = new ServerSession();

			session.callback_connect += OnConnected;
            session.callback_disconnect += OnDisConnected;
			session.callback_received += OnRecv;

			packetHandler.AddHandler(Handle_S_ENTER);
        }

        private void OnConnected()
        {
			UnityEngine.Debug.Log("Connected");
            
            var cts = new CancellationTokenSource();
            
            AsyncPacketUpdate(cts.Token).Forget();
            HeartBeat(cts.Token).Forget();

            C_ENTER packet = new C_ENTER();
			packet.ClientId = ConnectionId;
			session.Send(packet);
        }

        private void OnDisConnected()
        {
            cts.Cancel();
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

		public async UniTaskVoid AsyncPacketUpdate( CancellationToken token )
		{
            Action<IMessage> handler;

            while (!token.IsCancellationRequested)
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

        public async UniTaskVoid HeartBeat( CancellationToken token )
        {
            Protocol.C_HEARTBEAT heartbeat = new Protocol.C_HEARTBEAT();

            while (!token.IsCancellationRequested)
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
			Debug.Log(enter.Result);
		}
    }
}
