using Framework.Network;
using FrameWork.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public class MainConnection : Connection
{
	private Dictionary<string, GameObject> gameObjects = new();

	public MainConnection()
	{
		AddHandler(S_ENTER);
		AddHandler(INSTANTIATE_GAME_OBJECT);
		AddHandler(S_ADD_CLIENT);
		AddHandler(S_ADD_GAME_OBJECT);
		AddHandler(S_REMOVE_GAME_OBJECT);
		//AddHandler(S_SET_TRANSFORM);

		Debug.Log("Main Connection Constructor");
	}

	~MainConnection()
	{
		UnityEngine.Debug.Log("Main Connection Destructor");
	}

	public void S_ENTER(S_ENTER _packet)
	{
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

			Protocol.Vector3 rotation = new()
			{
				X = 0f,
				Y = 0f,
				Z = 0f
			};

			packet.Position = position;
			packet.Rotation = rotation;
			packet.PrefabName = "MarkerMan";

			Send(PacketManager.MakeSendBuffer(packet));
		}
	}

	public void INSTANTIATE_GAME_OBJECT(S_INSTANTIATE_GAME_OBJECT _packet)
	{
		Debug.Log("Instantiate GameObject : " + _packet.GameObjectId);

		if(myObjectId < 0) myObjectId = _packet.GameObjectId;
	}

	public void S_ADD_CLIENT(S_ADD_CLIENT _packet)
	{
		foreach (var clients in _packet.ClientInfos)
		{
			Debug.Log(clients.ClientId);
		}			
	}

	public void S_ADD_GAME_OBJECT(S_ADD_GAME_OBJECT _packet)
	{
		Debug.Log("Add GameObject : " + _packet);

		foreach (var gameObject in _packet.GameObjects)
		{
			UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
			UnityEngine.Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

			if (gameObject.Id == myObjectId)
			{
				var prefab = Resources.Load<GameObject>("Prefab/" + gameObject.PrefabName);
				prefab.GetComponent<NetworkTransform>().objectId = gameObject.Id;
				prefab.GetComponent<NetworkTransform>().isMine = true;

				var player = UnityEngine.Object.Instantiate(prefab, position, rotation);
				player.name = "MarkerMan_" + gameObject.Id;
				UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, position, Quaternion.identity);

				gameObjects.Add(gameObject.Id.ToString(), player);
			}

			else
			{
				if(gameObject.PrefabName == "Ride")
				{
					var prefab = Resources.Load<GameObject>(Define.PATH_MONSTER + gameObject.PrefabName);

					UnityEngine.Vector3 offset = UnityEngine.Vector3.up * prefab.transform.localScale.x * .5f;

					prefab.GetComponent<NetworkTransform>().objectId = gameObject.Id;
					prefab.GetComponent<NetworkTransform>().isMine = true;
					GameObject ride = UnityEngine.Object.Instantiate(prefab, position + offset, rotation);

					UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_SmokePuff, position + offset, rotation);

					gameObjects.Add(gameObject.Id.ToString(), ride);
				}

				else if (gameObject.PrefabName == "MarkerMan")
				{
					var prefab = Resources.Load<GameObject>("Prefab/" + gameObject.PrefabName);

					UnityEngine.Object.Destroy(prefab.GetComponent<PlayerController>());
					UnityEngine.Object.Destroy(prefab.transform.Search("Camera").gameObject);

					prefab.GetComponent<NetworkTransform>().objectId = gameObject.Id;
					prefab.GetComponent<NetworkTransform>().isMine = false;

					var player = UnityEngine.Object.Instantiate(prefab, position, rotation);
					player.name = "MarkerMan_" + gameObject.Id;
					UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, position, Quaternion.identity);

					gameObjects.Add(gameObject.Id.ToString(), player);
				}

				else
				{
					Monster rouge = new Rouge(MonsterType.MonsterA, "Monster", 100, 100, 10, 30, 1000, 10, 1, EffectType.Effect_Thunder, EffectType.Effect_Explosion);

					var dummy = UnityEngine.Object.FindObjectOfType<MonsterPool>().Spawn(rouge, position, rotation);

					UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(rouge.spawn, position, Quaternion.identity);

					dummy.name = gameObject.Id.ToString();

					if (!gameObjects.ContainsKey(dummy.name))
					{
						gameObjects.Add(dummy.name, dummy);
					}
				}
			}
		}
	}

	public void S_REMOVE_GAME_OBJECT(S_REMOVE_GAME_OBJECT _packet)
	{
		foreach (int gameObjectId in _packet.GameObjects)
		{
			if (!gameObjects.ContainsKey(gameObjectId.ToString())) continue;


			if (gameObjects[gameObjectId.ToString()].GetComponent<MonsterActor>() != null)
			{
				gameObjects[gameObjectId.ToString()].GetComponent<MonsterActor>().Die();
			}

			else
			{
				UnityEngine.Object.Destroy(gameObjects[gameObjectId.ToString()]);
			}

			gameObjects.Remove(gameObjectId.ToString());
		}
	}

	public void S_SET_TRANSFORM(S_SET_TRANSFORM _packet)
	{
		GameObject gameObject;

		if (!gameObjects.TryGetValue(_packet.GameObjectId.ToString(), out gameObject))
		{
			return;
		}

		if (gameObject == null)
		{
			return;
		}

		UnityEngine.Vector3 position = new(_packet.Position.X, _packet.Position.Y, _packet.Position.Z);
		Quaternion rotation = Quaternion.Euler(_packet.Rotation.X, _packet.Rotation.Y, _packet.Rotation.Z);

		gameObject.transform.localPosition = position;
		gameObject.transform.localRotation = rotation;
	}
}