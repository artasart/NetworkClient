using Framework.Network;
using Protocol;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameClientManager : MonoBehaviour
{
    #region Singleton

    public static GameClientManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<GameClientManager>();
            return instance;
        }
    }
    private static GameClientManager instance;

	#endregion



	#region Members

	public string ip = "192.168.0.104";
    public int port = 7777;

    private readonly Dictionary<string, GameObject> gameObjects = new();
    public int dummyCount = 0;

    private readonly Dictionary<string, Connection> dummyConnections = new();
    static int idGenerator = 0;

    private Connection mainConnection = null;

    private Transform go_Main;
    private Transform go_Dummy;

	#endregion



	private void Awake()
	{
        go_Main = this.transform.Search(nameof(go_Main));
        go_Dummy = this.transform.Search(nameof(go_Dummy));
	}

    private void Start()
	{
        GameManager.UI.StackPanel<Panel_Network>();
	}

    public async void CreateDummy()
	{
        IPEndPoint endPoint = new(IPAddress.Parse("192.168.0.104"), 7777);

        for (int i = 0; i < 20; i++)
        {
            DummyConnection connection = (DummyConnection)ConnectionManager.GetConnection<DummyConnection>();

            connection.AddHandler(connection.Handle_S_ENTER);
            connection.AddHandler(connection.Handle_S_INSTANTIATE_GAME_OBJECT);

            dummyConnections.Add(connection.ConnectionId, connection);
            bool success = await ConnectionManager.Connect(endPoint, connection);
            if (success)
            {
                Protocol.C_ENTER enter = new();
                enter.ClientId = "Dummy_" + idGenerator++.ToString();
                connection.Send(PacketManager.MakeSendBuffer(enter));
            }
            else
            {
                UnityEngine.Debug.Log("Enter Fail!");
            }
        }
    }

    public void DestroyDummy()
	{
        foreach (KeyValuePair<string, Connection> dummyConnection in dummyConnections)
        {
            dummyConnection.Value.Send(PacketManager.MakeSendBuffer(new Protocol.C_LEAVE()));

            //await Task.Delay(10);
        }

        dummyConnections.Clear();
    }

    public async void CreateMain(string _connectionId)
	{
        if (mainConnection != null)
        {
            return;
        }

        IPEndPoint endPoint = new(IPAddress.Parse("192.168.0.104"), 7777);
        MainConnection connection = (MainConnection)ConnectionManager.GetConnection<MainConnection>();

        mainConnection = connection;

        mainConnection.AddHandler(connection.Handle_S_ENTER);
        mainConnection.AddHandler(Test);
        mainConnection.AddHandler(AddGameObject);
        mainConnection.AddHandler(RemoveGameObject);
        mainConnection.AddHandler(SetTransform);

        bool success = await ConnectionManager.Connect(endPoint, connection);
        if (success)
        {
            Protocol.C_ENTER enter = new();
            enter.ClientId = "Main" + _connectionId;
            connection.Send(PacketManager.MakeSendBuffer(enter));
        }
    }

    public void DestroyMain()
	{
        mainConnection.Send(PacketManager.MakeSendBuffer(new Protocol.C_LEAVE()));
        mainConnection = null;
    }

    public void RemoveClient(Protocol.S_REMOVE_CLIENT pkt)
    {
        foreach (string clientId in pkt.ClientIds)
        {
            dummyConnections.Remove(clientId);
        }
    }

	public void AddGameObject( Protocol.S_ADD_GAME_OBJECT pkt )
    {
        foreach (S_ADD_GAME_OBJECT.Types.GameObjectInfo gameObject in pkt.GameObjects)
        {
            UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
            Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            Rouge rouge = new Rouge(MonsterType.MonsterA, "Rouge", 100, 100, 10, 30, 1000, 10, 1, EffectType.Effect_Thunder, EffectType.Effect_Explosion);

            FindObjectOfType<MonsterPool>().Spawn(rouge, position, rotation);
            FindObjectOfType<EffectPool>().Spawn(rouge.spawn, position, Quaternion.identity);

            var dummy = FindObjectOfType<MonsterPool>().GetGenerateMonster();

            dummy.name = gameObject.Id.ToString();

            if (!gameObjects.ContainsKey(dummy.name))
            {
                gameObjects.Add(dummy.name, dummy);
            }
        }
    }

    public void RemoveGameObject( Protocol.S_REMOVE_GAME_OBJECT pkt )
    {
		foreach (int gameObjectId in pkt.GameObjects)
		{
			gameObjects[gameObjectId.ToString()].GetComponent<MonsterActor>().Die();

			_ = gameObjects.Remove(gameObjectId.ToString());
		}
	}

	public void SetTransform( Protocol.S_SET_TRANSFORM pkt )
    {
        GameObject go;
        if(!gameObjects.TryGetValue(pkt.GameObjectId.ToString(), out go))
        {
            return;
        }

        if(go == null)
        {
            return;
        }

        UnityEngine.Vector3 position = new(pkt.Position.X, pkt.Position.Y, pkt.Position.Z);
        Quaternion rotation = Quaternion.Euler(pkt.Rotation.X, pkt.Rotation.Y, pkt.Rotation.Z);

        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
    }

    public void Test(Protocol.S_TEST pkt)
    {
        Debug.Log("Test");
    }
}
