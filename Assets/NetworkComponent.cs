using Framework.Network;
using MEC;
using System.Diagnostics;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkComponent : MonoBehaviour
	{
        public string connectionId = string.Empty;
        public Connection connection = null;

        public int objectId = 0;
        public bool isPlayer = false;
        public bool isMine = false;

		protected virtual void OnDestroy()
		{
			var effectPool = FindObjectOfType<EffectPool>();
			effectPool?.Spawn(EffectType.Effect_Thunder, this.transform.position, Quaternion.identity);
		}
	}
}