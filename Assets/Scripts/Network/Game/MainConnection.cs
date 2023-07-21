using Framework.Network;
using FrameWork.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public class MainConnection : Connection
{
	private Dictionary<string, GameObject> gameObjects = new();
	private int myObjectId = -1;

    public MainConnection()
	{
		AddHandler(S_ENTER);
		AddHandler(INSTANTIATE_GAME_OBJECT);
		AddHandler(S_ADD_CLIENT);
		AddHandler(S_ADD_GAME_OBJECT);
		AddHandler(S_REMOVE_GAME_OBJECT);
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
		myObjectId = _packet.GameObjectId;
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
		foreach (var gameObject in _packet.GameObjects)
		{
			UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
			UnityEngine.Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

			if (gameObject.Id == myObjectId)
			{
				var prefab = Resources.Load<GameObject>("Prefab/" + gameObject.PrefabName);
				prefab.GetComponent<NetworkObserver>().objectId = gameObject.Id;
				prefab.GetComponent<NetworkObserver>().isMine = true;
				prefab.GetComponent<NetworkObserver>().isPlayer = true;
                prefab.GetComponent<NetworkObserver>().connectionId = this.ConnectionId;

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

					prefab.GetComponent<NetworkObserver>().objectId = gameObject.Id;
					prefab.GetComponent<NetworkObserver>().isMine = true;
					GameObject ride = UnityEngine.Object.Instantiate(prefab, position + offset, rotation);

					UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_SmokePuff, position + offset, rotation);

					gameObjects.Add(gameObject.Id.ToString(), ride);
				}

				else if (gameObject.PrefabName == "MarkerMan")
				{
					var prefab = Resources.Load<GameObject>("Prefab/" + gameObject.PrefabName);
					prefab.GetComponent<NetworkObserver>().objectId = gameObject.Id;
					prefab.GetComponent<NetworkObserver>().isMine = false;
					prefab.GetComponent<NetworkObserver>().isPlayer = true;
                    prefab.GetComponent<NetworkObserver>().connectionId = this.ConnectionId;

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
}