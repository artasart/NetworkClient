using MEC;
using System.Diagnostics;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkComponent : MonoBehaviour
	{
		[HideInInspector] public string clientId;
		public int objectId = 0;
		public float interval = 0;
		public int totalStep;
		[HideInInspector] public float lerpSpeed = 10f;

		public bool isMine = false;
		public bool isPlayer = false;

		public bool isRecieved = false;

		protected Stopwatch stopwatch = new Stopwatch();
		public CoroutineHandle handle_update;

		protected virtual void Awake() 
		{
			interval = .05f;
			totalStep = 6;
		}

		protected virtual void Start() { }

		protected virtual void OnDestroy()
		{
			Timing.KillCoroutines(handle_update);

			var effectPool = FindObjectOfType<EffectPool>();

			effectPool?.Spawn(EffectType.Effect_Thunder, this.transform.position, Quaternion.identity);
		}
	}
}