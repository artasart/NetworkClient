using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

}

public class PoolData
{
	public int poolSize;
}

[System.Serializable]
public class EffectPoolData : PoolData
{
	public EffectType effectType;
}