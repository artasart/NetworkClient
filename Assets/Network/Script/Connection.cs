using Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FrameWork.Network
{
	public class Connection
	{
		#region Members

		public ServerSession session;

		public string ip;
		public int port;
		public string roomId;
		public string password;

		public int sessionId;

		public string result = string.Empty;
		public bool isConnected;

		public RealtimePacket packetManager = new RealtimePacket();
		public PacketQueue packetQueue = new PacketQueue();

		public Action callback_connect;
		public Action callback_enterFailed;
		public Action<object> callback_disconnect;

		#endregion



		#region Connection

		public Connection()
		{
			this.session = new ServerSession();
			this.isConnected = false;


			AddListener();
			AddHandler();

			AsyncPacketUpdate().Forget();
		}

		protected void AddListener()
		{
			packetManager.CustomHandler =
			(message, i) =>
			{
				packetQueue.Push(i, message);
			};

			session.callback_received +=
			(session, buffer) =>
			{
				packetManager.OnRecvPacket(session, buffer);
			};

			callback_connect += ConnectHandler;
			callback_disconnect += DisconnectHandler;
		}


		protected virtual void AddHandler()
		{	
			//packetManager.AddHandler(RealtimePacket.MsgId.PKT_S_DISCONNECT, S_DISCONNECT);
		}

		protected virtual void RemoveHandler()
		{
			packetManager.RemoveHandler(RealtimePacket.MsgId.PKT_S_ENTER, this);
			packetManager.RemoveHandler(RealtimePacket.MsgId.PKT_S_DISCONNECT, this);
		}


		private void ConnectHandler()
		{
			isConnected = true;
		}

		private void DisconnectHandler(object _endPoint)
		{
			if (_endPoint == null)
			{
				if (disconnected) return;
			}

			else
			{
				isConnected = false;
			}
		}


		public async UniTaskVoid AsyncPacketUpdate()
		{
			while (true)
			{
				try
				{
					var packets = packetQueue.PopAll();

					for (var i = 0; i < packets.Count; i++)
					{
						var handlers = packetManager.GetPacketHandler(packets[i].Id);

						if (handlers == null) continue;

						var values = new List<Action<IMessage>>(handlers.Values);

						for (var j = 0; j < values.Count; j++)
						{ 
							values[j].Invoke(packets[i].Message);
						}
					}
				}

				catch (Exception ex)
				{

				}

				await UniTask.Delay(TimeSpan.FromSeconds(0.02f));
			}
		}

		#endregion



		#region Enter

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


		#endregion



		#region Disconnect

		protected void S_DISCONNECT(IMessage _packet)
		{
			S_DISCONNECT packet = _packet as S_DISCONNECT;

			disconnected = true;
		}

		bool disconnected = false;

		public void Clear()
		{
			session.Disconnect();
			packetManager.Clear();

			RemoveHandler();
		}

		#endregion



		#region Leave

		public void Leave()
		{
			C_LEAVE packet = new C_LEAVE();
		}

		#endregion
	}
}
