using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class NetworkUtils
{
    public static Vector3 ProtocolVector3ToUnityVector3( Protocol.Vector3 vector3 )
    {
        return new Vector3(vector3.X, vector3.Y, vector3.Z);
    }

    public static Quaternion ProtocolVector3ToUnityQuaternion( Protocol.Vector3 vector3 )
    {
        return Quaternion.Euler(new Vector3(vector3.X, vector3.Y, vector3.Z));
    }

    public static Protocol.Vector3 UnityVector3ToProtocolVector3( Vector3 vector3 )
    {
        Protocol.Vector3 position = new()
        {
            X = vector3.x,
            Y = vector3.y,
            Z = vector3.z
        };

        return position;
    }
}
