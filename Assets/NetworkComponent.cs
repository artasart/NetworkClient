using Framework.Network;
using MEC;
using System.Diagnostics;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkComponent : MonoBehaviour
	{
		public Connection connection;
        public int objectId = -1;
        public bool isMine = false;
		public bool isPlayer = false;

		protected virtual void OnDestroy()
		{
			var effectPool = FindObjectOfType<EffectPool>();
			effectPool?.Spawn(EffectType.Effect_Thunder, this.transform.position, Quaternion.identity);
		}
	}
}