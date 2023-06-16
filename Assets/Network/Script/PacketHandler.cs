using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler
{
    Action<Protocol.S_ENTER> Handle_S_ENTER;

    void AddHandler(Action<Protocol.S_ENTER> handler)
    {
        Handle_S_ENTER += handler;
    }

    void RemoveHandler( Action<Protocol.S_ENTER> handler )
    {
        Handle_S_ENTER += handler;
    }

    void HandleMessage( Protocol.S_ENTER message)
    {
        Handle_S_ENTER?.Invoke(message);
    }
}