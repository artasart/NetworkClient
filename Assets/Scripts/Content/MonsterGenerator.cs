using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGenerator : MonoBehaviour
{
	[NonReorderable] public Queue<List<Monster>> monsterDeck = new Queue<List<Monster>>();

	public void Generate(float _radius)
	{
		List<Monster> monsters = new List<Monster>();

		for (int i = 0; i < Random.Range(10,20); i++)
		{
			Rouge rouge = new Rouge(MonsterType.MonsterA, "Rouge", 100, 100, 10, 30, 1000, 10, 1, EffectType.Effect_Thunder, EffectType.Effect_Explosion);

			monsters.Add(rouge);
		}

		monsterDeck.Enqueue(monsters);

		GenerateMonster(_radius);
	}

	public void Generate(int _quantity = 1)
	{
		List<Monster> monsters = new List<Monster>();

		for (int i = 0; i < _quantity; i++)
		{
			Rouge rouge = new Rouge(MonsterType.MonsterA, "Rouge", 100, 100, 10, 30, 1000, 10, 1, EffectType.Effect_Thunder, EffectType.Effect_Explosion);

			monsters.Add(rouge);
		}

		monsterDeck.Enqueue(monsters);

		GenerateMonster();
	}

	private void GenerateMonster()
	{
		while (monsterDeck.Count > 0)
		{
			var list = monsterDeck.Dequeue();

			foreach (var item in list)
			{
				Debug.Log($"{item.name} is spawned..!");

				var spawnPoint = GetRandomPosition();

				FindObjectOfType<MonsterPool>().Spawn(item, spawnPoint, Quaternion.identity);

				FindObjectOfType<EffectPool>().Spawn(item.spawn, spawnPoint, Quaternion.identity);
			}
		}
	}

	private void GenerateMonster(float _radius) => Timing.RunCoroutine(Co_GenerateMonster(_radius));

	IEnumerator<float> Co_GenerateMonster(float _radius)
	{
		while (monsterDeck.Count > 0)
		{
			var list = monsterDeck.Dequeue();

			var index = 0;

			foreach (var item in list)
			{
				float angle = index * (360f / list.Count) + Random.Range(-180f / list.Count, 180f / list.Count);
				float radius = _radius + Random.Range(-5f, 5f);

				Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

				Vector3 spawnPosition = transform.position + rotation * Vector3.forward * radius;

				yield return Timing.WaitForSeconds(Random.Range(0f, .1f));

				FindObjectOfType<MonsterPool>().Spawn(item, spawnPosition, GetRandomRotation());

				FindObjectOfType<EffectPool>().Spawn(item.spawn, spawnPosition, GetRandomRotation());

				index++;
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

	public Quaternion GetRandomRotation()
	{
		return Quaternion.Euler(new Vector3(0f, Random.Range(-360f, 360f), 0f));
	}
}
