using System;
using UnityEngine;

[Serializable]
public class Monster
{
	public MonsterType monsterType;
	public string name;
	public int health;
	public int MaxHealth;
	public int Damage;
	public int Defense;
	public int Experience;
	public int Gold;
	public int level;

	public EffectType spawn;
	public EffectType destroy;

	public Monster(MonsterType _monsterType, string _name, int _health, int _maxHealth, int _damage, int _defense, int _experience, int _gold, int _level)
	{
		monsterType = _monsterType;
		name = _name;
		health = _health;
		MaxHealth = _maxHealth;
		Damage = _damage;
		Defense = _defense;
		Experience = _experience;
		Gold = _gold;
		level = _level;
	}

	public virtual void Attack()
	{
		Debug.Log($"{name} has attacked.");
	}

	public virtual void Damaged(int _damage)
	{
		health -= _damage;

		if (health < 0) health = 0;

		Debug.Log($"{name} is damaged.. health : {health}");
	}
}

public class Savior : Monster
{
	public Savior(MonsterType _monsterType, string _name, int _health, int _maxHealth, int _damage, int _defense, int _experience, int _gold, int _level, EffectType _spawn, EffectType _destroy) :
	base(_monsterType,_name, _health, _maxHealth, _damage, _defense, _experience, _gold, _level)
	{
		spawn = _spawn;
		destroy = _destroy;
	}

	public override void Attack()
	{
		base.Attack();
	}

	public override void Damaged(int _damage)
	{
		base.Damaged(_damage);
	}
}

public class BladeMaster : Monster
{
	public BladeMaster(MonsterType _monsterType, string _name, int _health, int _maxHealth, int _damage, int _defense, int _experience, int _gold, int _level, EffectType _spawn, EffectType _destroy) :
	base(_monsterType, _name, _health, _maxHealth, _damage, _defense, _experience, _gold, _level)
	{
		spawn = _spawn;
		destroy = _destroy;
	}

	public override void Attack()
	{
		base.Attack();
	}

	public override void Damaged(int _damage)
	{
		base.Damaged(_damage);
	}
}

public class Rouge : Monster
{
	public Rouge(MonsterType _monsterType, string _name, int _health, int _maxHealth, int _damage, int _defense, int _experience, int _gold, int _level, EffectType _spawn, EffectType _destroy) :
	base(_monsterType, _name, _health, _maxHealth, _damage, _defense, _experience, _gold, _level)
	{
		spawn = _spawn;
		destroy = _destroy;
	}

	public override void Attack()
	{
		base.Attack();
	}

	public override void Damaged(int _damage)
	{
		base.Damaged(_damage);
	}
}

public class Raven : Monster
{
	public Raven(MonsterType _monsterType, string _name, int _health, int _maxHealth, int _damage, int _defense, int _experience, int _gold, int _level, EffectType _spawn, EffectType _destroy) :
	base(_monsterType, _name, _health, _maxHealth, _damage, _defense, _experience, _gold, _level)
	{
		spawn = _spawn;
		destroy = _destroy;
	}

	public override void Attack()
	{
		base.Attack();
	}

	public override void Damaged(int _damage)
	{
		base.Damaged(_damage);
	}
}

public class Barvarian : Monster
{
	public Barvarian(MonsterType _monsterType, string _name, int _health, int _maxHealth, int _damage, int _defense, int _experience, int _gold, int _level, EffectType _spawn, EffectType _destroy) :
	base(_monsterType, _name, _health, _maxHealth, _damage, _defense, _experience, _gold, _level)
	{
		spawn = _spawn;
		destroy = _destroy;
	}

	public override void Attack()
	{
		base.Attack();
	}

	public override void Damaged(int _damage)
	{
		base.Damaged(_damage);
	}
}