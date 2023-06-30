using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType
{
	Walk = 0,
	Run,
}
public class MonsterActor : Actor
{
	public Monster monster;

	public void Attack()
	{
		monster.Attack();
	}

	public void Damaged(int _damage)
	{
		monster.Damaged(_damage);
	}

	public void Die()
	{
		FindObjectOfType<MonsterPool>().RePool(this.gameObject, UnityEngine.Random.Range(0f, 1f));
	}
}
