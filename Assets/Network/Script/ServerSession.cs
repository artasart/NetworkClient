using Google.Protobuf;
using System;
using System.Net;
using UnityEngine;

namespace FrameWork.Network
{
	public class ServerSession : PacketSession
	{
		public Action<ArraySegment<byte>> callback_received = null;

		public Action callback_connect;
		public Action callback_disconnect;

		public void Send(IMessage _packet)
		{
			var message = "PKT_" + _packet.Descriptor.Name;
			var size = (ushort)_packet.CalculateSize();
			var sendBuffer = new byte[size + 4];

			RealtimePacket.MsgId msgId = (RealtimePacket.MsgId)Enum.Parse(typeof(RealtimePacket.MsgId), message);

			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(_packet.ToByteArray(), 0, sendBuffer, 4, size);

			Send(new ArraySegment<byte>(sendBuffer));
		}

		public override void OnConnected(EndPoint _endPoint)
		{
			callback_connect?.Invoke();
		}

		public override void OnDisconnected(EndPoint _endPoint)
		{
			callback_disconnect?.Invoke();
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;



            callback_received?.Invoke(buffer);
		}

		public override void OnSend(int _numOfBytes) {}
	}
}