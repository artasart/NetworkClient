using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGenerator : MonoBehaviour
{
	[NonReorderable] public Queue<List<Monster>> monsterDeck = new Queue<List<Monster>>();

	public void Generate()
	{
		List<Monster> monsters = new List<Monster>();

		for (int i = 0; i < 1; i++)
		{
			Rouge rouge = new Rouge(MonsterType.MonsterA, "Rouge", 100, 100, 10, 30, 1000, 10, 1);

			monsters.Add(rouge);
		}

		monsterDeck.Enqueue(monsters);

		GeneratorMonsters();

		monsters = null;
	}

	private void GeneratorMonsters()
	{
		while (monsterDeck.Count > 0)
		{
			var list = monsterDeck.Dequeue();

			foreach (var item in list)
			{
				Debug.Log($"{item.name} is spawned..!");

				FindObjectOfType<MonsterPool>().SpawnMonster(item.monsterType, GetRandomPosition(), Quaternion.identity);
			}
		}
	}

	public Vector3 GetRandomPosition()
	{
		float randomX = Random.Range(-25f, 25f);
		float randomY = 0f;
		float randomZ = Random.Range(-25f, 25f);

		return new Vector3(randomX, randomY, randomZ);
	}
}
