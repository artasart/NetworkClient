using Cysharp.Threading.Tasks;
using Framework.Network;
using Newtonsoft.Json.Linq;
using Protocol;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GameClientManager : MonoBehaviour
{
    #region Singleton

    public static GameClientManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<GameClientManager>();
            return instance;
        }
    }
    private static GameClientManager instance;

    #endregion

    public Client Client { get; private set; }

    public Dictionary<string, DummyClient> Dummies { get; private set; } = new();
    public int DummyId = 0;

    private readonly bool isLocal = false;

    private readonly string localAddress = "192.168.0.104";
    private readonly int localPort = 7777;

    private readonly string remoteAddress = "20.200.230.139";
    private readonly int remotePort = 32000;

    private readonly string query = "/Room/BASE";

    private void Start()
    {
        GameManager.UI.StackPanel<Panel_Network>();
    }

    public async Task<IPEndPoint> GetAddress()
    {
        if (isLocal)
        {
            return new(IPAddress.Parse(localAddress), localPort);
        }

        using UnityWebRequest webRequest = UnityWebRequest.Get("http://" + remoteAddress + ":" + remotePort + query);
        _ = await webRequest.SendWebRequest();
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

            return defaultPort != 0 ? new(IPAddress.Parse(address), defaultPort) : null;
        }
        else
        {
            return null;
        }
    }

    public async void Connect( string connectionId )
    {
        if (Client != null)
        {
            return;
        }

        IPEndPoint endPoint = await GetAddress();
        if (endPoint == null)
        {
            Debug.Log("GetAddress Fail!");
            return;
        }

        Client = (Client)ConnectionManager.GetConnection<Client>();

        bool success = await ConnectionManager.Connect(endPoint, Client);
        if (success)
        {
            Client.ClientId = connectionId;

            C_ENTER enter = new()
            {
                ClientId = connectionId
            };
            Client.Send(PacketManager.MakeSendBuffer(enter));

            GameManager.UI.PopPanel();
            GameManager.UI.StackPanel<Panel_Hint>();
        }
    }

    public void Disconnect()
    {
        if (Client == null)
        {
            return;
        }

        Client.Send(PacketManager.MakeSendBuffer(new C_LEAVE()));
        Client = null;
    }

    public async void AddDummy( int dummyNumber, string connectionId )
    {
        IPEndPoint endPoint = await GetAddress();
        if (endPoint == null)
        {
            Debug.Log("GetAddress Fail!");
            return;
        }

        for (int i = 0; i < dummyNumber; i++)
        {
            DummyClient dummy = (DummyClient)ConnectionManager.GetConnection<DummyClient>();

            bool success = await ConnectionManager.Connect(endPoint, dummy);
            if (success)
            {
                string dummyId = DummyId++.ToString();

                Dummies.Add(dummyId, dummy);

                dummy.ClientId = connectionId + "_Dummy_" + dummyId;

                C_ENTER enter = new()
                {
                    ClientId = dummy.ClientId
                };
                dummy.Send(PacketManager.MakeSendBuffer(enter));
            }
        }
    }

    public void RemoveDummy()
    {
        foreach (KeyValuePair<string, DummyClient> dummy in Dummies)
        {
            dummy.Value.Send(PacketManager.MakeSendBuffer(new C_LEAVE()));
            dummy.Value.Close();
        }

        Dummies.Clear();
    }
}
