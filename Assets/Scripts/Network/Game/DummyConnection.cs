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

        Send(PacketManager.MakeSendBuffer(packet));
    }

    public void Handle_S_INSTANTIATE_GAME_OBJECT( Protocol.S_INSTANTIATE_GAME_OBJECT enter )
    {
        //Protocol.C_LEAVE leave = new();
        //Send(PacketManager.MakeSendBuffer(leave));

        gameObjectId = enter.GameObjectId;

        Timing.RunCoroutine(Test());
    }

    IEnumerator<float> Test()
	{
        Protocol.C_SET_TRANSFORM st = new()
        {
            GameObjectId = gameObjectId
        };

        Protocol.Vector3 position = new();
        st.Position = position;
        position.X = p_x;
        position.Y = p_y;
        position.Z = p_z;

        Protocol.Vector3 rotation = new();
        st.Rotation = rotation;

        while (state == ConnectionState.NORMAL)
        {
            r_y = (r_y + UnityEngine.Random.Range(0, 2)) % 360;

            rotation.X = r_x;
            rotation.Y = r_y;
            rotation.Z = r_z;

            Send(PacketManager.MakeSendBuffer(st));

            yield return Timing.WaitForOneFrame;
        }
    }
}