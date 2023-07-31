using Cysharp.Threading.Tasks;
using Framework.Network;
using Protocol;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

	private readonly Dictionary<string, Connection> dummyConnections = new();

	public Connection mainConnection = null;

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

	public async Task<IPEndPoint> GetAddress()
	{
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://20.200.230.139:32000/"))
		{
            await webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                JObject jsonResponse = JObject.Parse(response);
                
				string address = jsonResponse["status"]["address"].ToString();
                
				int defaultPort = 0;
                JArray ports = (JArray)jsonResponse["status"]["ports"];
                foreach (JObject port in ports)
                {
                    if (port["name"].ToString() == "default")
                    {
                        defaultPort = port["port"].ToObject<int>();
                        break;
                    }
                }

				if (defaultPort != 0)
					return new(IPAddress.Parse(address), defaultPort);
				return null;
            }
            else
			{
                return null;
			}
        }

    }

	public async void CreateMain(string _connectionId)
	{
		if (mainConnection != null) return;

		IPEndPoint endPoint = await GetAddress();
		if(endPoint == null)
		{
            Debug.Log("GetAddress Fail!");
            return;
        }
		MainConnection connection = (MainConnection)ConnectionManager.GetConnection<MainConnection>();

		mainConnection = connection;

		bool success = await ConnectionManager.Connect(endPoint, connection);
		if (success)
		{
			C_ENTER enter = new C_ENTER();
			enter.ClientId = "Main" + _connectionId;
			connection.Send(PacketManager.MakeSendBuffer(enter));
		}
	}

	public void DestroyMain()
	{
		if (mainConnection == null) return;

		mainConnection.Send(PacketManager.MakeSendBuffer(new C_LEAVE()));
		mainConnection = null;
	}

	public void RemoveClient(S_REMOVE_CLIENT _packet)
	{
		foreach (string clientId in _packet.ClientIds)
		{
			dummyConnections.Remove(clientId);
		}
	}
}
