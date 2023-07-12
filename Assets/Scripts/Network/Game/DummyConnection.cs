using Cysharp.Threading.Tasks;
using Framework.Network;
using Protocol;
using System;
using MEC;
using System.Collections.Generic;

public class DummyConnection : Connection
{
    private int gameObjectId;
    private float p_x, p_y, p_z;
    private int r_x, r_y, r_z;

    public void Handle_S_ENTER( Protocol.S_ENTER enter )
    {
        C_INSTANTIATE_GAME_OBJECT packet = new();

        p_x = UnityEngine.Random.Range(-10f, 10f);
        p_y = 0f;
        p_z = UnityEngine.Random.Range(-10f, 10f);

        Protocol.Vector3 position = new()
        {
            X = p_x,
            Y = p_y,
            Z = p_z
        };

        r_x = 0;
        r_y = UnityEngine.Random.Range(-180, 180);
        r_z = 0;

        Protocol.Vector3 rotation = new()
        {
            X = r_x,
            Y = r_y,
            Z = r_z
        };

        packet.Position = position;
        packet.Rotation = rotation;
        packet.PrefabName = "MarkerMan";

        Send(PacketManager.MakeSendBuffer(packet));
    }

    public void Handle_S_INSTANTIATE_GAME_OBJECT( Protocol.S_INSTANTIATE_GAME_OBJECT enter )
    {
        gameObjectId = enter.GameObjectId;
    }
}