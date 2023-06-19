using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler
{
    public Dictionary<RealtimePacket.MsgId, Action<IMessage>> Handlers;
    
    Action<Protocol.S_ENTER> S_ENTER_Handler;

    public PacketHandler()
    {
        Handlers.Add(RealtimePacket.MsgId.PKT_S_ENTER, Handle_S_ENTER);
    }

    public void AddHandler( Action<Protocol.S_ENTER> handler )
    {
        S_ENTER_Handler += handler;
    }

    public void RemoveHandler( Action<Protocol.S_ENTER> handler )
    {
        S_ENTER_Handler -= handler;
    }

    public void Handle_S_ENTER( IMessage message )
    {
        S_ENTER_Handler.Invoke((Protocol.S_ENTER)message);
    }
}