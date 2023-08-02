using Framework.Network;
using FrameWork.Network;
using Protocol;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Client : Connection
{
	public string ClientId { get; set; }

	private Dictionary<string, GameObject> gameObjects = new();
	private int myGameObjectId = -1;

    public Client()
	{
		AddHandler(S_ENTER);
		AddHandler(INSTANTIATE_GAME_OBJECT);
		AddHandler(S_ADD_GAME_OBJECT);
		AddHandler(S_REMOVE_GAME_OBJECT);
	}

	public void S_ENTER(S_ENTER pkt)
	{
		if(pkt.Result != "SUCCESS")
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

	public void INSTANTIATE_GAME_OBJECT(S_INSTANTIATE_GAME_OBJECT pkt)
	{
		myGameObjectId = pkt.GameObjectId;
	}

	public void S_ADD_GAME_OBJECT(S_ADD_GAME_OBJECT _packet)
	{
		foreach (var gameObject in _packet.GameObjects)
		{
			UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
			UnityEngine.Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            var prefab = Resources.Load<GameObject>("Prefab/" + gameObject.PrefabName);
            prefab.GetComponent<NetworkObserver>().objectId = gameObject.Id;
			prefab.GetComponent<NetworkObserver>().isMine = gameObject.Id == myGameObjectId;
            prefab.GetComponent<NetworkObserver>().isPlayer = true;

            var player = UnityEngine.Object.Instantiate(prefab, position, rotation);

			player.GetComponent<NetworkObserver>().SetConnection(this);
            
			player.name = gameObject.Id.ToString();
            
			UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, position, Quaternion.identity);

            gameObjects.Add(gameObject.Id.ToString(), player);
		}
	}

	public void S_REMOVE_GAME_OBJECT(S_REMOVE_GAME_OBJECT pkt)
	{
		foreach (int gameObjectId in pkt.GameObjects)
		{
			if (!gameObjects.ContainsKey(gameObjectId.ToString())) continue;

            UnityEngine.Object.Destroy(gameObjects[gameObjectId.ToString()]);

            gameObjects.Remove(gameObjectId.ToString());
		}
	}
}