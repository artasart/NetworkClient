using MEC;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkComponent : MonoBehaviour
	{
		[HideInInspector] public string clientId;
		public int objectId = 0;
		public float interval = 0;
		[HideInInspector] public float lerpSpeed = 10f;

		public bool isMine = false;
		public bool isPlayer = false;

		public CoroutineHandle handle_update;

		protected virtual void Awake() { }

		protected virtual void Start() { }

		protected virtual void OnDestroy()
		{
			Timing.KillCoroutines(handle_update);

			var effectPool = FindObjectOfType<EffectPool>();

			effectPool?.Spawn(EffectType.Effect_Thunder, this.transform.position, Quaternion.identity);
		}
	}
}