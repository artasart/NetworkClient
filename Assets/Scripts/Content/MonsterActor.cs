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
		Timing.RunCoroutine(Co_Die());
	}

	IEnumerator<float> Co_Die()
	{
		yield return Timing.WaitForOneFrame;

		FindObjectOfType<MonsterPool>().RePool(this.gameObject);
	}
}
