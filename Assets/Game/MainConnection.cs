using Framework.Network;
using Protocol;
using UnityEngine;

public class MainConnection : Connection
{
    private static int idGenerator = 0;

    public MainConnection()
    {
        AddHandler(Handle_S_ENTER);

        connectedHandler += () =>
        {
            Protocol.C_ENTER enter = new()
            {
                ClientId = clientId
            };
            Send(PacketManager.MakeSendBuffer(enter));
        };
    }

    private void Handle_S_ENTER( Protocol.S_ENTER enter )
    {
        Debug.Log("ENTER : " + enter.Result);

        {
            C_GET_GAME_OBJECT packet = new();
            Send(PacketManager.MakeSendBuffer(packet));
        }

        {
            C_INSTANTIATE_GAME_OBJECT packet = new();

            Protocol.Vector3 position = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };

            Protocol.Vector3 rotation = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };

            packet.Position = position;
            packet.Rotation = rotation;

            Send(PacketManager.MakeSendBuffer(packet));
        }
    }
}