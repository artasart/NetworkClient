using MEC;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkComponent : MonoBehaviour
	{
		public string clientId;
		public int objectId = 0;
		public bool isMine = false;
		public bool isPlayer = false;

		public float lerpSpeed = 10f;

		public CoroutineHandle handle_update;

		protected virtual void Awake() { }

		protected virtual void Start() { }

		protected virtual void OnDestroy() 
		{
			Timing.KillCoroutines(handle_update);

			FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, this.transform.position, Quaternion.identity);
		}

		protected virtual void Dispose() { }
	}
}