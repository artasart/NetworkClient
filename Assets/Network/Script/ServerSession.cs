/****************************************************************

ServerSession.cs
연결된 서버에게 보내는 패킷 전송, 서버 연결 or 종료 될 때 불려지는 코드 

실시간 동기화 필수 Script

*****************************************************************/

using Google.Protobuf;
using System;
using System.Net;
using UnityEngine;

namespace FrameWork.Network
{
	/// <summary>
	/// 클라이언트에 연결되어있는 서버 세션 
	/// </summary>
	public class ServerSession : PacketSession
	{
		public Action<PacketSession, ArraySegment<byte>> callback_received = null;

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



		/// <summary>
		/// 서버에 연결 되었을 때 callback 실행 
		/// </summary>
		/// <param name="_endPoint"></param>
		public override void OnConnected(EndPoint _endPoint)
		{
			Debug.Log("On Connected");

			callback_connect?.Invoke();
		}



		/// <summary>
		/// 서버 연결이 끊어졌을 때 callback 실행
		/// </summary>
		/// <param name="_endPoint"></param>
		public override void OnDisconnected(EndPoint _endPoint)
		{
			Debug.Log("On Disconnected");

			callback_disconnect?.Invoke();
		}



		public override void OnRecvPacket(ArraySegment<byte> _buffer)
		{
			callback_received(this, _buffer);

			ushort count = 0;

			ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
			count += 2;
			ushort id = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + count);
			count += 2;

			Action<PacketSession, ArraySegment<byte>, ushort> action = null;

			//MakePacket<>();
		}



		public override void OnSend(int _numOfBytes)
		{

		}



	}
}