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

	public Client Client { get; private set; }

	private Transform go_Main;

	private void Awake()
	{
		go_Main = this.transform.Search(nameof(go_Main));
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

	public async void CreateMain(string connectionId)
	{
		if (Client != null) return;

		IPEndPoint endPoint = await GetAddress();
		if(endPoint == null)
		{
            Debug.Log("GetAddress Fail!");
            return;
        }
		Client connection = (Client)ConnectionManager.GetConnection<Client>();

		Client = connection;

		bool success = await ConnectionManager.Connect(endPoint, connection);
		if (success)
		{
			Client.ClientId = connectionId;

			C_ENTER enter = new C_ENTER();
			enter.ClientId = "Main" + connectionId;
			connection.Send(PacketManager.MakeSendBuffer(enter));
		}
	}

	public void DestroyMain()
	{
		if (Client == null) return;

		Client.Send(PacketManager.MakeSendBuffer(new C_LEAVE()));
		Client = null;
	}
}
