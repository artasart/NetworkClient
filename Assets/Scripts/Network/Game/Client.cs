using Framework.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public class Client : Connection
{
    public string ClientId { get; set; }

    private readonly Dictionary<string, GameObject> gameObjects = new();
    private HashSet<int> myGameObjectId = new();

    public Client()
    {
        AddHandler(OnEnter);
        AddHandler(OnInstantiateGameObject);
        AddHandler(OnAddGameObject);
        AddHandler(OnRemoveGameObject);
        AddHandler(OnDisconnected);
        AddHandler(DisplayPing);
        AddHandler(OnOwnerChanged);
    }

    ~Client()
    {
        Debug.Log("Client Destructor");
    }

    public void OnEnter( S_ENTER pkt )
    {
        if (pkt.Result != "SUCCESS")
        {
            Debug.Log(pkt.Result);
            return;
        }

        {
            C_GET_GAME_OBJECT packet = new();

            Send(PacketManager.MakeSendBuffer(packet));
        }

        {
            C_INSTANTIATE_GAME_OBJECT packet = new();

            packet.Type = Define.GAMEOBJECT_TYPE_PLAYER;

            Protocol.Vector3 position = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };
            packet.Position = position;

            Protocol.Vector3 rotation = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };
            packet.Rotation = rotation;

            packet.PrefabName = "MarkerMan";

            Send(PacketManager.MakeSendBuffer(packet));
        }
    }

    public void OnInstantiateGameObject( S_INSTANTIATE_GAME_OBJECT pkt )
    {
        myGameObjectId.Add(pkt.GameObjectId);
    }

    public void OnAddGameObject( S_ADD_GAME_OBJECT _packet )
    {
        foreach (S_ADD_GAME_OBJECT.Types.GameObjectInfo gameObject in _packet.GameObjects)
        {
            UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
            UnityEngine.Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            GameObject prefab = Resources.Load<GameObject>("Prefab/" + gameObject.PrefabName);

            GameObject player = UnityEngine.Object.Instantiate(prefab, position, rotation);

            player.GetComponent<NetworkObserver>().SetNetworkObject(
                this
                , gameObject.GameObjectId
                , myGameObjectId.Contains(gameObject.GameObjectId)
                , gameObject.Type == Define.GAMEOBJECT_TYPE_PLAYER
                , gameObject.OwnerId);

            player.name = gameObject.GameObjectId.ToString();

            UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, position, Quaternion.identity);

            gameObjects.Add(gameObject.GameObjectId.ToString(), player);
        }
    }

    public void OnRemoveGameObject( S_REMOVE_GAME_OBJECT pkt )
    {
        foreach (int gameObjectId in pkt.GameObjects)
        {
            if (!gameObjects.ContainsKey(gameObjectId.ToString()))
            {
                continue;
            }

            UnityEngine.Object.Destroy(gameObjects[gameObjectId.ToString()]);

            _ = gameObjects.Remove(gameObjectId.ToString());
        }
    }

    public void OnOwnerChanged(S_SET_GAME_OBJECT_OWNER pkt)
    {
        if(pkt.OwnerId == ClientId && !myGameObjectId.Contains(pkt.GameObjectId))
        {
            myGameObjectId.Add(pkt.GameObjectId);
        }
        else if(pkt.OwnerId != ClientId && myGameObjectId.Contains(pkt.GameObjectId))
        {
            myGameObjectId.Remove(pkt.GameObjectId);
        }
    }

    public void OnDisconnected( S_DISCONNECT pkt )
    {
        foreach (KeyValuePair<string, GameObject> gameObject in gameObjects)
        {
            UnityEngine.Object.Destroy(gameObject.Value);
        }

        gameObjects.Clear();
    }

    public void DisplayPing( Protocol.S_PING pkt )
    {
        Panel_NetworkInfo.Instance.SetPing((int)pingAverage);
    }
}