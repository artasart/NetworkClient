﻿using Google.Protobuf;
using System;
using System.Collections.Generic;

public class PacketHandler
{
    public Dictionary<ushort, Action<IMessage>> Handlers = new();
    Action<Protocol.S_ENTER> S_ENTER_Handler;
    Action<Protocol.S_REENTER> S_REENTER_Handler;
    Action<Protocol.S_ADD_CLIENT> S_ADD_CLIENT_Handler;
    Action<Protocol.S_REMOVE_CLIENT> S_REMOVE_CLIENT_Handler;
    Action<Protocol.S_DISCONNECT> S_DISCONNECT_Handler;
    Action<Protocol.S_INSTANTIATE_GAME_OBJECT> S_INSTANTIATE_GAME_OBJECT_Handler;
    Action<Protocol.S_ADD_GAME_OBJECT> S_ADD_GAME_OBJECT_Handler;
    Action<Protocol.S_REMOVE_GAME_OBJECT> S_REMOVE_GAME_OBJECT_Handler;
    Action<Protocol.S_SET_TRANSFORM> S_SET_TRANSFORM_Handler;

    public PacketHandler()
    {
        Handlers.Add(1, Handle_S_ENTER);
        Handlers.Add(3, Handle_S_REENTER);
        Handlers.Add(6, Handle_S_ADD_CLIENT);
        Handlers.Add(7, Handle_S_REMOVE_CLIENT);
        Handlers.Add(8, Handle_S_DISCONNECT);
        Handlers.Add(101, Handle_S_INSTANTIATE_GAME_OBJECT);
        Handlers.Add(103, Handle_S_ADD_GAME_OBJECT);
        Handlers.Add(104, Handle_S_REMOVE_GAME_OBJECT);
        Handlers.Add(106, Handle_S_SET_TRANSFORM);
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
        S_ENTER_Handler?.Invoke((Protocol.S_ENTER)message);
    }
    public void AddHandler( Action<Protocol.S_REENTER> handler )
    {
        S_REENTER_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_REENTER> handler )
    {
        S_REENTER_Handler -= handler;
    }
    public void Handle_S_REENTER( IMessage message )
    {
        S_REENTER_Handler?.Invoke((Protocol.S_REENTER)message);
    }
    public void AddHandler( Action<Protocol.S_ADD_CLIENT> handler )
    {
        S_ADD_CLIENT_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_ADD_CLIENT> handler )
    {
        S_ADD_CLIENT_Handler -= handler;
    }
    public void Handle_S_ADD_CLIENT( IMessage message )
    {
        S_ADD_CLIENT_Handler?.Invoke((Protocol.S_ADD_CLIENT)message);
    }
    public void AddHandler( Action<Protocol.S_REMOVE_CLIENT> handler )
    {
        S_REMOVE_CLIENT_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_REMOVE_CLIENT> handler )
    {
        S_REMOVE_CLIENT_Handler -= handler;
    }
    public void Handle_S_REMOVE_CLIENT( IMessage message )
    {
        S_REMOVE_CLIENT_Handler?.Invoke((Protocol.S_REMOVE_CLIENT)message);
    }
    public void AddHandler( Action<Protocol.S_DISCONNECT> handler )
    {
        S_DISCONNECT_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_DISCONNECT> handler )
    {
        S_DISCONNECT_Handler -= handler;
    }
    public void Handle_S_DISCONNECT( IMessage message )
    {
        S_DISCONNECT_Handler?.Invoke((Protocol.S_DISCONNECT)message);
    }
    public void AddHandler( Action<Protocol.S_INSTANTIATE_GAME_OBJECT> handler )
    {
        S_INSTANTIATE_GAME_OBJECT_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_INSTANTIATE_GAME_OBJECT> handler )
    {
        S_INSTANTIATE_GAME_OBJECT_Handler -= handler;
    }
    public void Handle_S_INSTANTIATE_GAME_OBJECT( IMessage message )
    {
        S_INSTANTIATE_GAME_OBJECT_Handler?.Invoke((Protocol.S_INSTANTIATE_GAME_OBJECT)message);
    }
    public void AddHandler( Action<Protocol.S_ADD_GAME_OBJECT> handler )
    {
        S_ADD_GAME_OBJECT_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_ADD_GAME_OBJECT> handler )
    {
        S_ADD_GAME_OBJECT_Handler -= handler;
    }
    public void Handle_S_ADD_GAME_OBJECT( IMessage message )
    {
        S_ADD_GAME_OBJECT_Handler?.Invoke((Protocol.S_ADD_GAME_OBJECT)message);
    }
    public void AddHandler( Action<Protocol.S_REMOVE_GAME_OBJECT> handler )
    {
        S_REMOVE_GAME_OBJECT_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_REMOVE_GAME_OBJECT> handler )
    {
        S_REMOVE_GAME_OBJECT_Handler -= handler;
    }
    public void Handle_S_REMOVE_GAME_OBJECT( IMessage message )
    {
        S_REMOVE_GAME_OBJECT_Handler?.Invoke((Protocol.S_REMOVE_GAME_OBJECT)message);
    }
    public void AddHandler( Action<Protocol.S_SET_TRANSFORM> handler )
    {
        S_SET_TRANSFORM_Handler += handler;
    }
    public void RemoveHandler( Action<Protocol.S_SET_TRANSFORM> handler )
    {
        S_SET_TRANSFORM_Handler -= handler;
    }
    public void Handle_S_SET_TRANSFORM( IMessage message )
    {
        S_SET_TRANSFORM_Handler?.Invoke((Protocol.S_SET_TRANSFORM)message);
    }
}