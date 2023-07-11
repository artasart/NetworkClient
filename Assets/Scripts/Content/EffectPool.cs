using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
	Effect_Thunder,
	Effect_Explosion,
	Effect_Puff,
	Effect_SmokePuff,
}


public class EffectPool : ObjectPool
{
	SerializableDictionary<EffectType, List<GameObject>> masterPool = new SerializableDictionary<EffectType, List<GameObject>>();

	[SerializeField] List<EffectPoolData> effectPools = new List<EffectPoolData>();

	Transform spawnParent;
	Transform poolParent;

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
		foreach (var item in effectPools)
		{
			List<GameObject> effectPool = new List<GameObject>();

			for (int i = 0; i < item.poolSize; i++)
			{
				GameObject effect = CreateEffect(item.effectType);
				effect.name = item.effectType.ToString() + "_" + (i + 1);

				effectPool.Add(effect);
			}

			masterPool.Add(item.effectType, effectPool);
		}
	}

	private GameObject CreateEffect(EffectType _effectType)
	{
		GameObject effectPrefab = GetEffectPrefab(_effectType);

		GameObject effect = Instantiate(effectPrefab, poolParent);
		effect.transform.localPosition = Vector3.zero;
		effect.transform.localRotation = Quaternion.identity;
		effect.transform.localScale = Vector3.one;
		effect.SetActive(false);

		return effect;
	}

	private GameObject GetEffectPrefab(EffectType _effectType)
	{
		return Resources.Load<GameObject>(Define.PATH_VFX + _effectType.ToString());
	}




	#region Core Methods

	public GameObject GetPool(object _effectType)
	{
		if (masterPool.TryGetValue((EffectType)_effectType, out List<GameObject> effectPool))
		{
			foreach (var item in effectPool)
			{
				if (!item.activeInHierarchy)
				{
					return item;
				}
			}
		}

		GameObject effect = CreateEffect((EffectType)_effectType);
		effect.name = _effectType.ToString() + "_" + (effectPool.Count + 1);

		effectPool.Add(effect);

		return effect;
	}


	public void RePool(GameObject _effect, float _delay = 0f) => Timing.RunCoroutine(Co_RePool(_effect, _delay));

	private IEnumerator<float> Co_RePool(GameObject _effect, float _delay = 0f)
	{
		yield return Timing.WaitForSeconds(_delay);

		_effect.SetActive(false);

		_effect.transform.SetParent(poolParent);
		_effect.transform.localPosition = Vector3.zero;
		_effect.transform.localRotation = Quaternion.identity;
		_effect.transform.localScale = Vector3.one;
	}


	public void Spawn(object _effectType, Vector3 position, Quaternion _rotation)
	{
		var effect = GetPool((EffectType)_effectType);

		effect.transform.SetParent(spawnParent);
		effect.transform.position = position;
		effect.transform.rotation = _rotation;

		effect.SetActive(true);
	}

	#endregion
}