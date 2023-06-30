using UnityEngine;
using System.Collections.Generic;
using MEC;

public enum MonsterType
{
	MonsterA,
	MonsterB,
	MonsterC,
	MonsterD,
	MonsterE
}

[System.Serializable]
public class MonsterPoolData
{
	public MonsterType monsterType;
	public int poolSize;
	public bool isRepool;
	public float repoolDelay;
}

public class MonsterPool : MonoBehaviour
{
	SerializableDictionary<MonsterType, List<GameObject>> masterPool = new SerializableDictionary<MonsterType, List<GameObject>>();
	
	[SerializeField] List<MonsterPoolData> monsterPools = new List<MonsterPoolData>();

	private Transform spawnParent;
	private Transform poolParent;

	private void Awake()
	{
		spawnParent = GameObject.Find("Spawn").transform;
		poolParent = GameObject.Find("Pool").transform;
	}

	private void Start()
	{
		CreatePool();
	}

	private void CreatePool()
	{
		foreach (MonsterPoolData poolData in monsterPools)
		{
			List<GameObject> monsterPool = new List<GameObject>();

			for (int i = 0; i < poolData.poolSize; i++)
			{
				GameObject monster = CreateMonster(poolData.monsterType);
				monster.name = poolData.monsterType.ToString() + "_" + (i + 1);

				monsterPool.Add(monster);
			}

			masterPool.Add(poolData.monsterType, monsterPool);
		}
	}

	private GameObject CreateMonster(MonsterType _monsterType)
	{
		GameObject monsterPrefab = GetMonsterPrefab(_monsterType);

		GameObject monster = Instantiate(monsterPrefab, poolParent);
		monster.transform.localPosition = Vector3.zero;
		monster.transform.localRotation = Quaternion.identity;
		monster.transform.localScale = Vector3.one;
		monster.SetActive(false);

		return monster;
	}

	private GameObject GetMonsterPrefab(MonsterType _monsterType)
	{
		return Resources.Load<GameObject>(Define.PATH_MONSTER + _monsterType.ToString());
	}

	#region Core Methods

	public GameObject GetPool(MonsterType _monsterType)
	{
		if (masterPool.TryGetValue(_monsterType, out List<GameObject> monsterPool))
		{
			foreach (var item in monsterPool)
			{
				if (!item.activeInHierarchy)
				{
					return item;
				}
			}
		}

		GameObject monster = CreateMonster(_monsterType);
		monster.name = _monsterType.ToString() + "_" + (monsterPool.Count + 1);

		monsterPool.Add(monster);

		return monster;
	}


	public void RePool(GameObject _monster, float _delay) => Timing.RunCoroutine(Co_RePool(_monster, _delay));

	private IEnumerator<float> Co_RePool(GameObject _monster, float _delay = 0f)
	{
		yield return Timing.WaitForSeconds(_delay);

		_monster.SetActive(false);

		_monster.transform.SetParent(poolParent);
		_monster.transform.localPosition = Vector3.zero;
		_monster.transform.localRotation = Quaternion.identity;
		_monster.transform.localScale = Vector3.one;
	}

	public void SpawnMonster(MonsterType _monsterType, Vector3 position, Quaternion _rotation)
	{
		var monster = GetPool(_monsterType);

		monster.transform.SetParent(spawnParent);
		monster.transform.position = position;
		monster.transform.rotation = _rotation;

		monster.SetActive(true);
	}

	#endregion
}