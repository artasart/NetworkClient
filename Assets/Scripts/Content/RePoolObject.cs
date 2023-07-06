using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class RePoolObject : MonoBehaviour
{
	ParticleSystem mainParticle;

	private void OnEnable()
    {
        Timing.RunCoroutine(Co_CheckAlive(), "CheckLive");
    }

	private void Awake()
	{
		mainParticle = GetComponent<ParticleSystem>();
	}

	private IEnumerator<float> Co_CheckAlive()
	{
		yield return Timing.WaitUntilTrue(() => !mainParticle.isPlaying);

        FindObjectOfType<EffectPool>().RePool(this.gameObject);
    }
}
