using FrameWork.Network;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class NetworkUtils
{
    public static Vector3 ProtocolVector3ToUnityVector3( Protocol.Vector3 proPos )
    {
        return new Vector3(proPos.X, proPos.Y, proPos.Z);
    }

    public static Quaternion ProtocolVector3ToUnityQuaternion(Protocol.Vector3 proPos)
    {
        return Quaternion.Euler(new Vector3(proPos.X, proPos.Y, proPos.Z));
    }

    public static Protocol.Vector3 UnityVector3ToProtocolVector3( Vector3 proPos )
    {
        Protocol.Vector3 position = new Protocol.Vector3();
        position.X = proPos.x;
        position.Y = proPos.y;
        position.Z = proPos.z;

        return position;
    }
}
