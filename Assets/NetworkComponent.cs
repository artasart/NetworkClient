using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkComponent : MonoBehaviour
	{
		[Header("Network")]
		public string clientId;
		public int objectId;
		public bool isMine;

		protected virtual void Awake() { }

		protected virtual void Start() { }

		protected virtual void OnDestroy() { }

		protected virtual void Dispose() { }
	}
}