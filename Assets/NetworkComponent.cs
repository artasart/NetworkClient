using UnityEngine;

namespace FrameWork.Network
{
    public class NetworkComponent : MonoBehaviour
    {
        public Client Client { get; set; }
        public int objectId = -1;
        public bool isMine = false;
        public bool isPlayer = false;

        protected virtual void OnDestroy()
        {
            EffectPool effectPool = FindObjectOfType<EffectPool>();
            effectPool?.Spawn(EffectType.Effect_Thunder, transform.position, Quaternion.identity);
        }
    }
}